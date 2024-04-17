namespace BookyWooks.Messaging.Events;
public record CheckStockEvent : IntegrationEvent
{
  public CheckStockEvent(Guid orderId, IEnumerable<OrderItemEventDto> orderItems)
  {
        OrderId = orderId;
        OrderItems = orderItems; 
  }
  public Guid OrderId { get; init; }
  public IEnumerable<OrderItemEventDto> OrderItems { get; init; }
}


