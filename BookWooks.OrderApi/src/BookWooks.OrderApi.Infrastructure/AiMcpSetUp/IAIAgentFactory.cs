using Microsoft.Agents.AI;

namespace BookWooks.OrderApi.Infrastructure.AiMcpSetUp;
public interface IAIAgentFactory
{
  AIAgent CreateAgent(ChatClientAgentOptions chatClientAgentOptions);
  Task<IMcpClient> CreateMcpClientAsync();
}
