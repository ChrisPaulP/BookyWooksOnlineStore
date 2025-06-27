using BookWooks.OrderApi.Infrastructure.AIClients;
using BookWooks.OrderApi.Infrastructure.Options;
using BookWooks.OrderApi.UseCases.Orders.AiServices;
using BookWooks.OrderApi.UseCases.Products;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using BookWooks.OrderApi.Infrastructure.AiServices;
using ModelContextProtocol.Protocol;
#pragma warning disable SKEXP0001 

internal class OrderAiService : BaseClient, IOrderAiService<ProductDto>
{
  private readonly OpenAIOptions _options;

  public OrderAiService(IOptions<OpenAIOptions> options)
  {
    _options = options.Value;
  }

  private async Task<(IMcpClient mcpClient, Kernel kernel)> CreateClientAndKernelAsync()
  {
    var mcpClient = await CreateMcpClientAsync();
    var tools = await mcpClient.ListToolsAsync();
    DisplayTools(tools);

    var kernel = CreateKernelWithChatCompletionService(_options.OpenAiApiKey, _options.ChatModelId);
    kernel.Plugins.AddFromFunctions("Tools", tools.Select(aiFunction => aiFunction.AsKernelFunction()));
    return (mcpClient, kernel);
  }

  public async Task<string> CustomerSupportAsync(string query)
  {
    var (mcpClient, kernel) = await CreateClientAndKernelAsync();
    await using (mcpClient)
    {
      var executionSettings = new OpenAIPromptExecutionSettings
      {
        Temperature = 0,
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
      };

      ReadResourceResult resource = await mcpClient.ReadResourceAsync($"vectorStore://support/{query}");

      ChatHistory chatHistory = [];
      chatHistory.AddUserMessage(resource.ToChatMessageContentItemCollection());
      chatHistory.AddUserMessage(query);

      var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
      var result = await chatCompletion.GetChatMessageContentAsync(chatHistory, executionSettings, kernel);

      return result.Content ?? string.Empty;
    }
  }

  public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string query)
  {
    var (mcpClient, kernel) = await CreateClientAndKernelAsync();
    await using (mcpClient)
    {
      var arguments = new KernelArguments
      {
        ["prompt"] = query,
        ["collection"] = "products"
      };

      var result = await kernel.InvokeAsync(
          pluginName: "Tools",
          functionName: "ProductSearchTool_Search",
          arguments: arguments
      );

      if (result.GetValue<object>() is not JsonElement jsonElement)
        return Enumerable.Empty<ProductDto>();

      if (!jsonElement.TryGetProperty("content", out var contentArray) ||
          contentArray.ValueKind != JsonValueKind.Array ||
          contentArray.GetArrayLength() == 0)
        return Enumerable.Empty<ProductDto>();

      var text = contentArray[0].GetProperty("text").GetString();

      if (string.IsNullOrWhiteSpace(text))
        return Enumerable.Empty<ProductDto>();

      return JsonSerializer.Deserialize<IEnumerable<ProductDto>>(text) ?? Enumerable.Empty<ProductDto>();
    }
  }
}
