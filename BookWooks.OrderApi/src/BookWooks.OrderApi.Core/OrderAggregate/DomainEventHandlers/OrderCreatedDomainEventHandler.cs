using BookWooks.OrderApi.Core.OrderAggregate.Entities;

namespace BookWooks.OrderApi.Core.OrderAggregate.Handlers;
internal class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedEvent>
{
  private readonly ILogger<OrderCreatedDomainEventHandler> _logger;
  private readonly IOrderIntegrationEventService _orderIntegrationEventService;

  public OrderCreatedDomainEventHandler(ILogger<OrderCreatedDomainEventHandler> logger, IOrderIntegrationEventService orderIntegrationEventService)
  {
    _logger = logger;
    _orderIntegrationEventService = orderIntegrationEventService;
  }

  public async Task Handle(OrderCreatedEvent domainEvent, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling Contributed Deleted event for {contributorId}", domainEvent.NewOrder.Message);
    var checkStockAvailabilityIntegrationEvent = new CheckBookStockIntegrationEvent(domainEvent.NewOrder.Id, new List<OrderItem>());
    await _orderIntegrationEventService.AddAndSaveEventAsync(checkStockAvailabilityIntegrationEvent);

    // TODO: do meaningful work here
    await Task.Delay(1);
  }
}
