using BookWooks.OrderApi.Infrastructure.AiServices.Interfaces;
#pragma warning disable SKEXP0001 
namespace BookWooks.OrderApi.Infrastructure.AiServices;
public class AiOperations : IAiOperations
{
  public async Task<ReadResourceResult> GetResourceAsync(IMcpClient mcpClient, string query)
  {
    return await mcpClient.ReadResourceAsync($"{AiServiceConstants.SupportVectorStorePrefix}{query}");
  }
}
