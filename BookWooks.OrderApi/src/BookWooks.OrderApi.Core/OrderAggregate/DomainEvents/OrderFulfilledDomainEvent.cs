using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookyWooks.SharedKernel.DomainEventsDispatching;

namespace BookWooks.OrderApi.Core.OrderAggregate.Events;
public class OrderFulfilledDomainEvent : DomainEventBase
{
  public Order FulfilledOrder { get; set; }

  public OrderFulfilledDomainEvent(Order fulfilledOrder)
  {
    FulfilledOrder = fulfilledOrder;
  }
}
