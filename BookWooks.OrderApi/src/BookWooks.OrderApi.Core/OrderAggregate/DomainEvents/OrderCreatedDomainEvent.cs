using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookyWooks.Messaging.Contracts.Events;
using BookyWooks.Messaging.Messages.InitialMessage;
using BookyWooks.SharedKernel.DomainEventsDispatching;
using BookyWooks.SharedKernel.Messages;
using Newtonsoft.Json;

namespace BookWooks.OrderApi.Core.OrderAggregate.Events;
/// <summary>
/// A domain event that is dispatched whenever a contributor is deleted.
/// The DeleteContributorService is used to dispatch this event.
/// </summary>
public class OrderCreatedDomainEvent : DomainEventBase, IConvert
{
  //public Guid NewOrderId { get; set; }
  public Order NewOrder { get; set; }

  public OrderCreatedDomainEvent(Order newOrder) //Guid newOrderId)
  {
    //NewOrderId = newOrderId;
    NewOrder  = newOrder;
  }
  public OutboxMessage ToOutboxMessage()
  {
    var orderStockList = NewOrder.OrderItems.Select(orderItem => new OrderItemEventDto(orderItem.OrderId.Value, orderItem.Quantity.Value));//.ToList();
    var orderCreatedMessage = new OrderCreatedMessage(NewOrder.OrderId.Value, NewOrder.CustomerId.Value, NewOrder.OrderTotal, orderStockList);

    return new OutboxMessage(this.Id,nameof(OrderCreatedMessage),JsonConvert.SerializeObject(orderCreatedMessage),this.DateOccurred
    );
  }
}
