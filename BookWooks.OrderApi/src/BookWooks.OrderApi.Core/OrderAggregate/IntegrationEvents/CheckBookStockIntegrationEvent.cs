namespace BookWooks.OrderApi.Core.OrderAggregate.IntegrationEvents;
public record CheckBookStockIntegrationEvent : IntegrationEventBase
{
  public CheckBookStockIntegrationEvent(Guid orderId, IEnumerable<OrderItem> orderItems)
  {
    OrderId = orderId;
    OrderItems = orderItems; 
  }
  public Guid OrderId { get; init; }
  public IEnumerable<OrderItem> OrderItems { get; init; }
}


