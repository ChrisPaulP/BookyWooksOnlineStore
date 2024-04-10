using BookWooks.OrderApi.Core.OrderAggregate.Entities;
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
    _logger.LogInformation("Handling Contributed Deleted event for {contributorId}", domainEvent.NewOrder.Message);
    var checkStockAvailabilityIntegrationEvent = new CheckBookStockIntegrationEvent(domainEvent.NewOrder.Id);//, new List<OrderItem>());

    //await _integrationEventService.Publish(checkStockAvailabilityIntegrationEvent);
    await _massTransitService.Send(checkStockAvailabilityIntegrationEvent);
    //await _orderIntegrationEventService.SaveChangesAsync();

    // TODO: do meaningful work here
    await Task.Delay(1);
  }
}
