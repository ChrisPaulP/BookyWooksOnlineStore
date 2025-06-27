using Azure.Core.Pipeline;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
namespace BookWooks.OrderApi.Infrastructure.AIClients;
public class BaseClient
{
  //protected static Task<IMcpClient> CreateMcpClientAsync(Kernel? kernel = null)
  //{
  //  // Create and return the MCP client
  //  return McpClientFactory.CreateAsync(
  //      clientTransport: new StdioClientTransport(new StdioClientTransportOptions
  //      {
  //        Name = "BookWooks.MCPServer",
  //        Command = GetMCPServerPath(), // Path to the MCPServer executable

  //      }),
  //      clientOptions: null
  //   );
  //}
  protected static Task<IMcpClient> CreateMcpClientAsync(Kernel? kernel = null)
  {
    // Use the Docker Compose service name and port
    var mcpServerHost = Environment.GetEnvironmentVariable("MCPSERVER__HOST") ?? "bookwooks.mcpserver";
    var mcpServerPort = Environment.GetEnvironmentVariable("MCPSERVER__PORT") ?? "8181";
    //var mcpServerUrl = $"http://{mcpServerHost}:{mcpServerPort}";
    var mcpServerUrl = "http://bookwooks.mcpserver:8181/";

    return McpClientFactory.CreateAsync(
                new SseClientTransport(
                    new SseClientTransportOptions
                    {
                      Endpoint = new Uri(mcpServerUrl)
                    }
                )
            );
  }
  protected static Kernel CreateKernelWithChatCompletionService(string openAIApiKey, string chatModelId)
  {
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.Services.AddOpenAIChatCompletion(modelId: chatModelId, apiKey: openAIApiKey);

    return kernelBuilder.Build();
  }
  protected static void DisplayTools(IList<McpClientTool> tools)
  {
    Console.WriteLine("Available MCP tools:");
    foreach (var tool in tools)
    {
      Console.WriteLine($"- Name: {tool.Name}, Description: {tool.Description}");
    }
    Console.WriteLine();
  }
}
