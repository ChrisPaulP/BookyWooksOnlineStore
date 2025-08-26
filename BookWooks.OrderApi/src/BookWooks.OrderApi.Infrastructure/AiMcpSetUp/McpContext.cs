namespace BookWooks.OrderApi.Infrastructure.AiMcpSetUp;
public sealed class McpContext : IAsyncDisposable
{
  public IMcpClient Client { get; }
  public Kernel Kernel { get; }

  public McpContext(IMcpClient client, Kernel kernel)
  {
    Client = client ?? throw new ArgumentNullException(nameof(client));
    Kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
  }
  public ValueTask DisposeAsync() => Client.DisposeAsync();
}
