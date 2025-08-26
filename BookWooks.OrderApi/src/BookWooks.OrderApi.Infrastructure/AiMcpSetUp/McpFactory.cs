#pragma warning disable SKEXP0001 
namespace BookWooks.OrderApi.Infrastructure.AiMcpSetUp;
public class McpFactory : IMcpFactory
{
  private readonly OpenAIOptions _options;
  private const int MaxRetries = 3;
  private McpFactory(OpenAIOptions options)
  {
    _options = options;
  }
  public static McpFactory Create(OpenAIOptions options) =>  new McpFactory(options);

  public async Task<McpContext> CreateClientAndKernelAsync()
  {
    var client = await CreateMcpClientAsync();

    var tools = await client.ListToolsAsync();
    DisplayTools(tools);

    var kernel = CreateKernelWithChatCompletionService(_options.OpenAiApiKey, _options.ChatModelId);
    kernel.Plugins.AddFromFunctions(AiServiceConstants.ToolsPluginName, tools.Select(aiFunction => aiFunction.AsKernelFunction()));
    return new McpContext(client, kernel);
  }

  private static Task<IMcpClient> CreateMcpClientAsync(Kernel? kernel = null)
  {
    // Use the Docker Compose service name and port
    var mcpServerHost = Environment.GetEnvironmentVariable("MCPSERVER__HOST") ?? "bookwooks.mcpserver";
    var mcpServerPort = Environment.GetEnvironmentVariable("MCPSERVER__PORT") ?? "8181";
    var mcpServerUrl = $"http://{mcpServerHost}:{mcpServerPort}";
    Console.WriteLine($"[DEBUG] Connecting to MCP server at {mcpServerUrl}");

    return McpClientFactory.CreateAsync(
    new SseClientTransport(new SseClientTransportOptions { Endpoint = new Uri(mcpServerUrl), ConnectionTimeout = TimeSpan.FromMinutes(2) }));
  }

  private static Kernel CreateKernelWithChatCompletionService(string openAIApiKey, string chatModelId)
  {
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.Services.AddOpenAIChatCompletion(modelId: chatModelId, apiKey: openAIApiKey);

    return kernelBuilder.Build();
  }
  private static void DisplayTools(IList<McpClientTool> tools)
  {
    Console.WriteLine("Available MCP tools:");
    foreach (var tool in tools)
    {
      Console.WriteLine($"- Name: {tool.Name}, Description: {tool.Description}");
    }
    Console.WriteLine();
  }
}
