#pragma warning disable SKEXP0001 
#pragma warning disable OPENAI001

namespace BookWooks.OrderApi.Infrastructure.AiMcpSetUp;
public class AIAgentFactory : IAIAgentFactory
{
  private readonly OpenAIClient _client;
  private readonly string _chatModelId;
  private readonly McpServerOptions _mcpOptions;

  private const int MaxRetries = 3;
  public AIAgentFactory(OpenAIOptions aiOptions, McpServerOptions mcpOptions)
  {
    _client = new OpenAIClient(aiOptions.OpenAiApiKey);
    _chatModelId = aiOptions.ChatModelId;
    _mcpOptions = mcpOptions;
  }

  public AIAgent CreateAgent(ChatClientAgentOptions options) => _client.GetChatClient(_chatModelId).CreateAIAgent(options);
  
  public async Task<IMcpClient> CreateMcpClientAsync()
  {
    var mcpServerHost = Environment.GetEnvironmentVariable("MCPSERVER__HOST") ?? "bookwooks.mcpserver";
    var mcpServerPort = Environment.GetEnvironmentVariable("MCPSERVER__PORT") ?? "8181";
    var endpoint = new Uri($"http://{mcpServerHost}:{mcpServerPort}");
    Console.WriteLine($"[DEBUG] Connecting to MCP server at {endpoint}");

    return await McpClientFactory.CreateAsync(
        new SseClientTransport(new SseClientTransportOptions
        {
          Endpoint = endpoint,
          ConnectionTimeout = TimeSpan.FromMinutes(2)
        }));
  }
}
