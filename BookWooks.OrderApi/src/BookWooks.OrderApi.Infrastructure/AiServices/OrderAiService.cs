#pragma warning disable SKEXP0001 

using System.Text.RegularExpressions;
using BookWooks.OrderApi.Core.OrderAggregate.Specifications;
using BookWooks.OrderApi.Infrastructure.AiMcpSetUp;
using BookWooks.OrderApi.Infrastructure.AiServices.Interfaces;

internal class OrderAiService : ICustomerSupportService, IProductSearchService
{
  private readonly IMcpFactory _mcpFactory;
  private readonly IAiOperations _aiOperations;
  private readonly IReadRepository<Product> _productRepository;
  public OrderAiService(IMcpFactory mcpFactory, IAiOperations aiOperations, IReadRepository<Product> productRepository)
  {
    _mcpFactory = mcpFactory;
    _aiOperations = aiOperations;
    _productRepository = productRepository;
  }

  public async Task<string> CustomerSupportAsync(string query)
  {
    await using var context = await _mcpFactory.CreateClientAndKernelAsync();

    var resource = await _aiOperations.GetResourceAsync(context, query);
    var chatHistory = new ChatHistory();
    chatHistory.AddUserMessage(resource.ToChatMessageContentItemCollection());
    chatHistory.AddUserMessage(query);

    return await _aiOperations.GetCompletionAsync(context, chatHistory);
  }
  public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string query)
  {
    await using var context = await _mcpFactory.CreateClientAndKernelAsync();

    var count = ExtractCount(query, fallback: 5);

    var productIds = await DeserializeAsync<IEnumerable<Guid>>(
        context.Kernel,
        AiServiceConstants.ToolsPluginName,
        "ProductSearchTool_Search",
        new KernelArguments
        {
          ["prompt"] = query,
          ["collection"] = AiServiceConstants.ProductsCollection,
          ["topN"] = count
        });

    if (productIds == null || !productIds.Any())
      return [];

    var products = await _productRepository.FindAllAsync(
       new ProductsByIdsSpecification(productIds));

    var dtoResults = products.Select(p => new ProductDto
    {
      Id = p.ProductId.Value,
      Name = p.Name.Value,
      Description = p.Description.Value,
      Price = p.Price.Value
    });

    // Maintain MCP order (vector ranking)
    var ordered = productIds
        .Join(dtoResults, id => id, dto => dto.Id, (_, dto) => dto)
        .Take(count);

    return ordered;
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

  private static async Task<T?> DeserializeAsync<T>(Kernel kernel, string pluginName, string functionName, KernelArguments arguments)
  {
      var result = await kernel.InvokeAsync(pluginName, functionName, arguments);
      Console.WriteLine($"[DEBUG] Kernel result: {JsonSerializer.Serialize(result.GetValue<object>())}");

      if (result.GetValue<object>() is JsonElement json)
      {    
        if (!JsonHelpers.TryGetNonEmptyArrayProperty(json, "content", out var contentArray))
        {
          Console.WriteLine("[ERROR] No content array found in response");
          return default;
        }

        var texts = contentArray.EnumerateArray()
            .Where(item => item.TryGetProperty("text", out var textProp))
            .Select(item => item.GetProperty("text").GetString())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(text => text!.Trim());

        var combinedText = string.Join(Environment.NewLine, texts);
        if (string.IsNullOrWhiteSpace(combinedText))
        {
          Console.WriteLine("[ERROR] No valid text content found");
          return default;
        }
          return JsonSerializer.Deserialize<T>(combinedText);
      }
      return default;
    }
  }

