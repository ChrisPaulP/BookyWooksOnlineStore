using BookWooks.OrderApi.Infrastructure.AiMcpSetUp;

namespace BookWooks.OrderApi.Infrastructure.AiServices.Interfaces;
public interface IAiOperations
{
  Task<ReadResourceResult> GetResourceAsync(McpContext context, string query);
  Task<string> GetCompletionAsync(McpContext context, ChatHistory history);
}
