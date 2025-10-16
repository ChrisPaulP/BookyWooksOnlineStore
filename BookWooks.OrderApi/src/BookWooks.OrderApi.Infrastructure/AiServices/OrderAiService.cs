#pragma warning disable SKEXP0001 


using System.Text.RegularExpressions;
using BookWooks.OrderApi.Core.OrderAggregate.Specifications;
using BookWooks.OrderApi.Infrastructure.AiMcpSetUp;
using BookWooks.OrderApi.Infrastructure.AiServices.Interfaces;


using TextContent = Microsoft.Extensions.AI.TextContent;
internal class OrderAiService : ICustomerSupportService, IProductSearchService
{
  private readonly IAIAgentFactory _aiAgentFactory;
  private readonly IAiOperations _aiOperations;
  private readonly IReadRepository<Product> _productRepository;
  public OrderAiService(IAIAgentFactory aiAgentFactory, IAiOperations aiOperations, IReadRepository<Product> productRepository)
  {
    _aiAgentFactory = aiAgentFactory;
    _aiOperations = aiOperations;
    _productRepository = productRepository;
  }

  public async Task<string> CustomerSupportAsync(string query)
  {
    var responseBuilder = new StringBuilder();

    await using var context = await _aiAgentFactory.CreateMcpClientAsync();

    var resource = await _aiOperations.GetResourceAsync(context, query);

    List<ChatMessage> messages = new();
 
    messages.Add(new ChatMessage(ChatRole.User,
        resource.Contents
            .OfType<TextResourceContents>()
            .Select(t => new TextContent(t.Text))
            .ToArray()));

    var agent = _aiAgentFactory.CreateAgent(new ChatClientAgentOptions()
    {
      Name = "CustomerSupportAgent",
      Instructions = """
                You are a helpful customer support assistant. Use the provided context to answer the user's question.
                If the context does not contain the information needed, respond with "I'm sorry, I don't have that information."
                """,
      ChatOptions = new ChatOptions()
    });

    await foreach (var update in agent.RunStreamingAsync(messages))
    {
        responseBuilder.Append(update.Text);
    }
    
    return responseBuilder.ToString();
  }

  public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string query)
  {
    var mcpClient = await _aiAgentFactory.CreateMcpClientAsync();

    var tools = await mcpClient.ListToolsAsync();
    JsonElement schema = AIJsonUtilities.CreateJsonSchema(typeof(ProductIdsSearchResult));
    var count = ExtractCount(query, fallback: 5);
    ChatOptions chatOptions = new()
    {
      ResponseFormat = ChatResponseFormatJson.ForJsonSchema(
        schema: schema,
        schemaName: "ProductIdsSearchResult",
        schemaDescription: "Information about product details"),
      Tools = tools.Cast<AITool>().ToList()
    };
    var chatClientAgentOptions = new ChatClientAgentOptions()
    {
      Name = "ProductSearchAgent",
      Instructions = $"""
                Use the following tool: ProductSearchTool_Search (Searches product embeddings and returns top product IDs), Strict = False
                The collection name is 'products'.
                The topN = {count}
                Respond in ProductIdsSearchResult JSON format.
                """,
      ChatOptions = chatOptions
    };

    var agent = _aiAgentFactory.CreateAgent(chatClientAgentOptions);
    var response = await agent.RunAsync(query);

    var productIdsSearchResult = response.Deserialize<ProductIdsSearchResult>(JsonSerializerOptions.Web);

    var products = await _productRepository.FindAllAsync(
       new ProductsByIdsSpecification(productIdsSearchResult.ProductIds));

    var productDtos = products.Select(p => new ProductDto
    {
      Id = p.ProductId.Value,
      Name = p.Name.Value,
      Description = p.Description.Value,
      Price = p.Price.Value
    });

    var orderedProducts = productIdsSearchResult.ProductIds
        .Join(productDtos, id => id, dto => dto.Id, (_, dto) => dto)
        .Take(count);

    return orderedProducts;
  }
  public int ExtractCount(string prompt, int fallback = AiServiceConstants.DefaultProductCount)
  {
    var numericMatch = Regex.Match(prompt, @"\b\d+\b");
    if (numericMatch.Success && int.TryParse(numericMatch.Value, out var num) && num > 0)
      return num;

    return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
      ["one"] = 1,
      ["two"] = 2,
      ["three"] = 3,
      ["four"] = 4,
      ["five"] = 5,
      ["six"] = 6,
      ["seven"] = 7,
      ["eight"] = 8,
      ["nine"] = 9,
      ["ten"] = 10,
      ["dozen"] = 12,
      ["couple"] = 2,
      ["few"] = 3
    }
        .Where(x => Regex.IsMatch(prompt, $@"\b{x.Key}\b", RegexOptions.IgnoreCase))
        .Select(x => x.Value)
        .FirstOrDefault(fallback);
  }
}

