namespace BookWooks.OrderApi.Infrastructure.AiMcpSetUp;
public interface IMcpFactory
{
  Task<McpContext> CreateClientAndKernelAsync();
}
