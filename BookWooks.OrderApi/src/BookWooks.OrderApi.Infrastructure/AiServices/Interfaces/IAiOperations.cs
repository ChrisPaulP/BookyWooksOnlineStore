using BookWooks.OrderApi.Infrastructure.AiMcpSetUp;

namespace BookWooks.OrderApi.Infrastructure.AiServices.Interfaces;
public interface IAiOperations
{
  Task<ReadResourceResult> GetResourceAsync(IMcpClient mcpClient, string query);
}
