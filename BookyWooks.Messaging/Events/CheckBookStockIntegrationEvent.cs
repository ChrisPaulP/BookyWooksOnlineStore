namespace BookyWooks.Messaging.Events;
public record CheckBookStockIntegrationEvent : IntegrationEvent
{
  public CheckBookStockIntegrationEvent(Guid orderId) //  IEnumerable<OrderItem> orderItems
  {
    OrderId = orderId;
    //OrderItems = orderItems; 
  }
  public Guid OrderId { get; init; }
  //public IEnumerable<OrderItem> OrderItems { get; init; }
}


