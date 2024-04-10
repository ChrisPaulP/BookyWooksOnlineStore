namespace BookWooks.OrderApi.Infrastructure.MassTransit;
public class OrderMassTransitService : IMassTransitService
{
  private readonly IPublishEndpoint _publishEndpoint;
  private readonly ILogger<OrderMassTransitService> _logger;
  public OrderMassTransitService(

      ILogger<OrderMassTransitService> logger,
      IPublishEndpoint publishEndpoint)

  {
    _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }
  public async Task Send<T>(T message) where T : class
  {
    await _publishEndpoint.Publish(message);
  }
}
