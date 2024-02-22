namespace BookWooks.OrderApi.Core.OrderAggregate.Events;
public class OrderFulfilledEvent : DomainEventBase
{
  public Order FulfilledOrder { get; set; }

  public OrderFulfilledEvent(Order fulfilledOrder)
  {
    FulfilledOrder = fulfilledOrder;
  }
}
