using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookWooks.OrderApi.Core.OrderAggregate.Events;
using BookyWooks.Messaging.Constants;
using BookyWooks.Messaging.Contracts.Events;
using BookyWooks.Messaging.MassTransit;
using BookyWooks.Messaging.Messages.InitialMessage;



namespace BookWooks.OrderApi.Core.OrderAggregate.Handlers;
internal class OrderCreatedDomainEventHandler : INotificationHandler<Events.OrderCreatedDomainEvent>
{
  private readonly ILogger<OrderCreatedDomainEventHandler> _logger;
  private readonly IMassTransitService _massTransitService;

  public OrderCreatedDomainEventHandler(ILogger<OrderCreatedDomainEventHandler> logger, IMassTransitService massTransitService)
  {
    _logger = logger;
    _massTransitService = massTransitService;
  }

  public async Task Handle(Events.OrderCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling Order Created Domain event for {new order}", domainEvent.NewOrder.Message);
    var orderStockList = domainEvent.NewOrder.OrderItems
   .Select(orderItem => new OrderItemEventDto(orderItem.ProductId, orderItem.Quantity));
    var orderCreatedMessage = new OrderCreatedMessage(domainEvent.NewOrder.Id, domainEvent.NewOrder.CustomerId, domainEvent.NewOrder.OrderTotal, orderStockList);

 
    await _massTransitService.Send(orderCreatedMessage, QueueConstants.CreateOrderMessageQueueName);

  }
}
