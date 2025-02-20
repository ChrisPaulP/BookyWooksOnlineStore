using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookyWooks.SharedKernel.DomainEventsDispatching;

namespace BookWooks.OrderApi.Core.OrderAggregate.Events;
public class OrderFulfilledEvent : DomainEventBase
{
  public Order FulfilledOrder { get; set; }

  public OrderFulfilledEvent(Order fulfilledOrder)
  {
    FulfilledOrder = fulfilledOrder;
  }
}
