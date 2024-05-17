namespace BookWooks.OrderApi.Infrastructure.MassTransit;
public class OrderMassTransitService : IMassTransitService
{
  private readonly IPublishEndpoint _publishEndpoint;
  private readonly ISendEndpointProvider _sendEndpointProvider;
  private readonly ILogger<OrderMassTransitService> _logger;
  public OrderMassTransitService(

      ILogger<OrderMassTransitService> logger,
      IPublishEndpoint publishEndpoint,
      ISendEndpointProvider sendEndpointProvider)

  {
    _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _sendEndpointProvider = sendEndpointProvider;
  }
  public async Task Publish<T>(T message) where T : class
  {
    await _publishEndpoint.Publish(message);
  }
  public async Task Send<T>(T message, string queueName) where T : class
  {
    var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{queueName}"));

    await sendEndpoint.Send<T>(message);
  }
}
