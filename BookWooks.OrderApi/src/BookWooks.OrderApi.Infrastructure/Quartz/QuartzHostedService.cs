using Microsoft.Extensions.Hosting;

namespace BookWooks.OrderApi.Infrastructure.Quartz;
internal class QuartzHostedService : IHostedService
{
  private readonly ILogger<QuartzHostedService> _logger;
  private readonly IConfiguration _configuration;

  public QuartzHostedService(ILogger<QuartzHostedService> logger, IConfiguration configuration)
  {
    _logger = logger;
    _configuration = configuration;
  }

  public Task StartAsync(CancellationToken cancellationToken)
  {
    long? interval = _configuration.GetValue<long?>("Quartz:InternalProcessingPoolingInterval");
    QuartzStartup.Initialize(_logger, interval);
    return Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    QuartzStartup.StopQuartz();
    return Task.CompletedTask;
  }
}
