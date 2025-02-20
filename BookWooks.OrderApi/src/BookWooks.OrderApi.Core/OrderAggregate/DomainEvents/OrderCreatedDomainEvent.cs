using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookyWooks.SharedKernel.DomainEventsDispatching;

namespace BookWooks.OrderApi.Core.OrderAggregate.Events;
/// <summary>
/// A domain event that is dispatched whenever a contributor is deleted.
/// The DeleteContributorService is used to dispatch this event.
/// </summary>
public class OrderCreatedDomainEvent : DomainEventBase
{
  //public Guid NewOrderId { get; set; }
  public Order NewOrder { get; set; }

  public OrderCreatedDomainEvent(Order newOrder) //Guid newOrderId)
  {
    //NewOrderId = newOrderId;
    NewOrder  = newOrder;
  }
}
