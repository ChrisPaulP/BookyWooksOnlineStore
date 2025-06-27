//#pragma warning disable SKEXP0001
//using Microsoft.SemanticKernel.Connectors.OpenAI;
//using Microsoft.SemanticKernel;
//using ModelContextProtocol.Client;
//using ModelContextProtocol.Protocol;
//using Microsoft.SemanticKernel.ChatCompletion;
//using BookWooks.OrderApi.Infrastructure.Extensions;
//using BookyWooks.SharedKernel.AIInterfaces;
//using Microsoft.Extensions.Options;
//using BookWooks.OrderApi.Infrastructure.Options;
//using System.Text.RegularExpressions;
//using Microsoft.Identity.Client;

//namespace BookWooks.OrderApi.Infrastructure.AIClients;
//internal class BookClient : BaseClient, IAIClient
//{
//  private readonly OpenAIOptions _options;

//  public BookClient(IOptions<OpenAIOptions> options)
//  {
//    _options = options.Value;
//  }
//  public async Task<string> RunAsync(string genre)
//  {
//    Console.WriteLine($"Running the {nameof(BookClient)} sample.");

//    await using IMcpClient mcpClient = await CreateMcpClientAsync();

//    IList<McpClientTool> tools = await mcpClient.ListToolsAsync();
//    DisplayTools(tools);
//    IList<McpClientPrompt> prompts = await mcpClient.ListPromptsAsync();
//    DisplayPrompts(prompts);

//    // Step 3: Register tools with the kernel (optional if you're using MCP directly)
//    Kernel kernel = CreateKernelWithChatCompletionService(_options.OpenAiApiKey, _options.ChatModelId);
//    kernel.Plugins.AddFromFunctions("Tools", tools.Select(aiFunction => aiFunction.AsKernelFunction()));

//    OpenAIPromptExecutionSettings executionSettings = new()
//    {
//      FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
//    };

//    GetPromptResult bookRecommendationPrompt = await mcpClient.GetPromptAsync("BookRecommendations", new Dictionary<string, object?>() { ["genre"] = genre });

//    string response = string.Join(Environment.NewLine, bookRecommendationPrompt.Messages[0].Content.Text ?? string.Empty);
//    BookRecommendationResult recommendationResult = ParseBookRecommendation(response);

//    var reservationPrompt = await mcpClient.GetPromptAsync("ReserveStock", new Dictionary<string, object?> () { ["bookTitle"] = recommendationResult.BookTitle });

//    var chatHistory = new ChatHistory(bookRecommendationPrompt.ToChatMessageContents());
//    chatHistory.AddRange(reservationPrompt.ToChatMessageContents());

//    IChatCompletionService chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

//    ChatMessageContent result = await chatCompletion.GetChatMessageContentAsync(chatHistory,
//    executionSettings: executionSettings,
//    kernel: kernel);
//    Console.WriteLine(result);
//    return "Complete";
//  }

//  private static void DisplayPrompts(IList<McpClientPrompt> prompts)
//  {
//    Console.WriteLine("Available MCP prompts:");
//    foreach (var prompt in prompts)
//    {
//      Console.WriteLine($"- Name: {prompt.Name}, Description: {prompt.Description}");
//    }
//    Console.WriteLine();
//  }
//  public static BookRecommendationResult ParseBookRecommendation(string response)
//  {
//    //var match = Regex.Match(
//    //    response,
//    //    @"This book will be perfect for you: (?<title>.+?)\. The (?<genre>\w+) genre is very popular at the moment\.",
//    //    RegexOptions.IgnoreCase);

//    //if (!match.Success)
//    //  throw new InvalidOperationException("Response format did not match expected template.");

//    //return new BookRecommendationResult
//    //{
//    //  BookTitle = match.Groups["title"].Value.Trim(),
//    //  Genre = match.Groups["genre"].Value.Trim()
//    //};
//    return JsonConvert.DeserializeObject<BookRecommendationResult>(response)
//               ?? throw new InvalidOperationException("Invalid JSON format");
//  }
//  public class BookRecommendationResult
//  {
//    public string BookTitle { get; set; } = string.Empty;
//    //public string Genre { get; set; } = string.Empty;
//  }
//}
