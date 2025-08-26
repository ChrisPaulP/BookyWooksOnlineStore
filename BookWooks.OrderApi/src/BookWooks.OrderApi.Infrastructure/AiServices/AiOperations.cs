using BookWooks.OrderApi.Infrastructure.AiMcpSetUp;
using BookWooks.OrderApi.Infrastructure.AiServices.Interfaces;
using ModelContextProtocol;
#pragma warning disable SKEXP0001 
namespace BookWooks.OrderApi.Infrastructure.AiServices;
public class AiOperations : IAiOperations
{
  public async Task<ReadResourceResult> GetResourceAsync(McpContext context, string query)
  {
    return await context.Client.ReadResourceAsync($"{AiServiceConstants.SupportVectorStorePrefix}{query}");
  }
  //public async Task<ReadResourceResult> GetResourceAsync(McpContext context, string query)
  //{
  //  try
  //  {
  //    // Log the attempt
  //    Console.WriteLine($"[DEBUG] Attempting to read resource with query: {query}");
  //    Console.WriteLine($"[DEBUG] Using vector store prefix: {AiServiceConstants.SupportVectorStorePrefix}");

  //    // Validate context and client
  //    if (context?.Client == null)
  //    {
  //      throw new InvalidOperationException("MCP Context or Client is null");
  //    }

  //    // List available resources to check if our target exists
  //    var resources = await context.Client.ListResourcesAsync();
  //    Console.WriteLine($"[DEBUG] Available resources: {JsonSerializer.Serialize(resources.Select(r => r.Uri))}");

  //    // List resource templates to verify configuration
  //    var templates = await context.Client.ListResourceTemplatesAsync();
  //    Console.WriteLine($"[DEBUG] Available templates: {JsonSerializer.Serialize(templates.Select(t => new
  //    {
  //      t.Name,
  //      t.UriTemplate,
  //      t.Description
  //    }), new JsonSerializerOptions { WriteIndented = true })}");

  //    // Check for the specific template we need
  //    var vectorStoreTemplate = templates.FirstOrDefault(t => t.UriTemplate?.Contains("vectorStore://") == true);
  //    if (vectorStoreTemplate != null)
  //    {
  //      Console.WriteLine($"[DEBUG] Found vector store template:");
  //      Console.WriteLine($"[DEBUG] - Name: {vectorStoreTemplate.Name}");
  //      Console.WriteLine($"[DEBUG] - URI Template: {vectorStoreTemplate.UriTemplate}");
  //      Console.WriteLine($"[DEBUG] - Description: {vectorStoreTemplate.Description}");
  //    }
  //    else
  //    {
  //      Console.WriteLine("[WARN] No vector store template found!");
  //    }

  //    // Extract collection from URI template pattern
  //    var uriTemplatePattern = vectorStoreTemplate?.UriTemplate ?? "";
  //    Console.WriteLine($"[DEBUG] URI Template pattern: {uriTemplatePattern}");

  //    // Extract expected collection from our prefix
  //    var expectedCollection = AiServiceConstants.SupportVectorStorePrefix
  //        .Replace("vectorStore://", "")
  //        .TrimEnd('/');
  //    Console.WriteLine($"[DEBUG] Expected collection from prefix: {expectedCollection}");

  //    // Attempt to read the resource
  //    var resourceUri = $"{AiServiceConstants.SupportVectorStorePrefix}{query}";
  //    Console.WriteLine($"[DEBUG] Attempting to read resource at URI: {resourceUri}");

  //    try
  //    {
  //      var result = await context.Client.ReadResourceAsync(resourceUri);
  //      Console.WriteLine($"[DEBUG] Successfully read resource:");
  //      Console.WriteLine($"[DEBUG] - Content count: {result.Contents?.Count ?? 0}");
  //      if (result.Contents?.Any() == true)
  //      {
  //        foreach (var content in result.Contents)
  //        {
  //          if (content is TextResourceContents textContent)
  //          {
  //            Console.WriteLine($"[DEBUG] - Text content length: {textContent.Text?.Length ?? 0}");
  //            Console.WriteLine($"[DEBUG] - Text preview: {textContent.Text?.Substring(0, Math.Min(100, textContent.Text?.Length ?? 0))}...");
  //          }
  //        }
  //      }
  //      return result;
  //    }
  //    catch (McpException mcpEx)
  //    {
  //      Console.WriteLine($"[ERROR] Failed to read resource at {resourceUri}");
  //      Console.WriteLine($"[ERROR] Message: {mcpEx.Message}");
  //      throw; // Re-throw to be handled by outer catch
  //    }
  //  }
  //  catch (McpException ex)
  //  {
  //    Console.WriteLine($"[ERROR] MCP Exception while reading resource:");
  //    Console.WriteLine($"[ERROR] Message: {ex?.Message ?? "No message available"}");

  //    // Provide a fallback response
  //    var defaultResource = new TextResourceContents
  //    {
  //      Text = @"Here is some general information that might help:
  //- To track your order, log into your account and visit the 'Order History' section
  //- For returns, you have 30 days from the delivery date
  //- Contact customer service at support@bookwooks.com
  //- Our phone support is available Monday-Friday, 9am-5pm",
  //      MimeType = "text/plain",
  //      Uri = "fallback://customer-support"
  //    };

  //    return new ReadResourceResult
  //    {
  //      Contents = new List<ResourceContents> { defaultResource }
  //    };
  //  }
  //  catch (Exception ex)
  //  {
  //    Console.WriteLine($"[ERROR] Unexpected error while reading resource: {ex.Message}");
  //    Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
  //    throw;
  //  }
  //  }
  public async Task<string> GetCompletionAsync(McpContext context, ChatHistory history)
  {
    var result = await context.Kernel.GetRequiredService<IChatCompletionService>()
        .GetChatMessageContentAsync(history,
            new OpenAIPromptExecutionSettings
            {
              Temperature = 0,
              FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
            },
            context.Kernel);

    return result.Content ?? string.Empty;
  }
}
