namespace BookWooks.OrderApi.Core.OrderAggregate.Events;
public class OrderCreatedDomainEvent : DomainEventBase, IConvertToOutBoxMessage
{
  public Order NewOrder { get; set; }

  public OrderCreatedDomainEvent(Order newOrder) => NewOrder  = newOrder;
  
  public OutboxMessage ToOutboxMessage()
  {
    var orderStockList = NewOrder.OrderItems.Select(orderItem => new OrderItemEventDto(orderItem.ProductId.Value, orderItem.Quantity.Value));//.ToList();
    var orderCreatedMessage = new OrderCreatedMessage(NewOrder.OrderId.Value, NewOrder.CustomerId.Value, NewOrder.OrderTotal, orderStockList);

    return new OutboxMessage(this.Id,nameof(OrderCreatedMessage),JsonConvert.SerializeObject(orderCreatedMessage),this.DateOccurred
    );
  }
}
