#pragma warning disable SKEXP0001 

using System.Text.RegularExpressions;
using BookWooks.OrderApi.Infrastructure.AiMcpSetUp;
using BookWooks.OrderApi.Infrastructure.AiServices.Interfaces;

internal class OrderAiService : ICustomerSupportService, IProductSearchService
{
  private readonly IMcpFactory _mcpFactory;
  private readonly IAiOperations _aiOperations;
  public OrderAiService(IMcpFactory mcpFactory, IAiOperations aiOperations)
  {
    _mcpFactory = mcpFactory;
    _aiOperations = aiOperations;
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

    var products = await DeserializeAsync<IEnumerable<ProductDto>>(
        context.Kernel,
        AiServiceConstants.ToolsPluginName,
        "ProductSearchTool_Search",
        new KernelArguments
        {
          ["prompt"] = $"The user is asking for a single product or a number of products. Please fulfill this request. Take into account the number of products they have asked for :\n<input>\n{query}\n</input>",
          ["collection"] = AiServiceConstants.ProductsCollection
        });

    return products?.Take(count) ?? [];
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

