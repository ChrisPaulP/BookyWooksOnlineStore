namespace BookWooks.OrderApi.Infrastructure.Consumers;
public class OrderFailedEventConsumer : IConsumer<OrderFailedEvent>
{
  private readonly ILogger<OrderFailedEventConsumer> _logger;

  public OrderFailedEventConsumer(ILogger<OrderFailedEventConsumer> logger)
  {
    _logger = logger;
  }

  public async Task Consume(ConsumeContext<OrderFailedEvent> context)
  {
    // TODO: Send email to customer
    await Task.Delay(1000);
     _logger.LogInformation("Order Failed notification sent to customer with Id: {CustomerId} for order Id: {MessageOrderId}",
        context.Message.customerId, context.Message.orderId);
  }
}
