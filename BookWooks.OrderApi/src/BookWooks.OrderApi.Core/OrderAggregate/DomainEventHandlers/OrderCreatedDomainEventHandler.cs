using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookWooks.OrderApi.Core.OrderAggregate.Events;
using BookyWooks.Messaging.Events;
using BookyWooks.Messaging.MassTransit;

namespace BookWooks.OrderApi.Core.OrderAggregate.Handlers;
internal class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedEvent>
{
  private readonly ILogger<OrderCreatedDomainEventHandler> _logger;
  private readonly IMassTransitService _massTransitService;

  public OrderCreatedDomainEventHandler(ILogger<OrderCreatedDomainEventHandler> logger, IMassTransitService massTransitService)
  {
    _logger = logger;
    _massTransitService = massTransitService;
  }

  public async Task Handle(OrderCreatedEvent domainEvent, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling Order Created Domain event for {new order}", domainEvent.NewOrder.Message);
    var orderStockList = domainEvent.NewOrder.OrderItems
   .Select(orderItem => new OrderItemEventDto(orderItem.ProductId, orderItem.Quantity));
    var checkStockAvailabilityIntegrationEvent = new CheckStockEvent(domainEvent.NewOrder.Id, orderStockList);

 
    await _massTransitService.Send(checkStockAvailabilityIntegrationEvent);

  }
}
