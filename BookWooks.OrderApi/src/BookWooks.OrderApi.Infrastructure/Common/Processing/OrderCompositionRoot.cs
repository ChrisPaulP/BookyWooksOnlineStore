namespace BookWooks.OrderApi.Infrastructure.Common.Processing;
public static class OrderCompositionRoot
{
  private static IServiceProvider? _serviceProvider;

  public static void SetServiceProvider(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
  }

  public static IServiceScope BeginLifetimeScope()
  {
    if (_serviceProvider == null)
    {
      throw new InvalidOperationException("Service provider is not set.");
    }
    return _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
  }
}
