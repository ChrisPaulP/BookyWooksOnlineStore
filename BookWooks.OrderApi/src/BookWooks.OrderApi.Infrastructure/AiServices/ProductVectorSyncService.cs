using Microsoft.Extensions.Hosting;

namespace BookWooks.OrderApi.Infrastructure.AiServices;
public sealed class ProductVectorSyncService : BackgroundService
{
  //private readonly ProductEmbeddingSyncService _sync;
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<ProductVectorSyncService> _logger;
  private readonly TimeSpan _interval = TimeSpan.FromHours(24); // adjust as needed

  public ProductVectorSyncService(IServiceProvider serviceProvider, ILogger<ProductVectorSyncService> logger)
  {
    //_sync = sync;
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Product vector sync service started");

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        using (var scope = _serviceProvider.CreateScope())
        {
          var syncService = scope.ServiceProvider.GetRequiredService<ProductEmbeddingSyncService>();
          await syncService.PopulateAsync(AiServiceConstants.ProductsCollection, stoppingToken);
        }
        _logger.LogInformation("Product vector store sync completed");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error syncing product vector store");
      }
      await Task.Delay(_interval, stoppingToken);
    }
  }
}

