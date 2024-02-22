
using BookWooks.OrderApi.Core.Orders;
using BookWooks.OrderApi.Infrastructure.Data;
using BookWooks.OrderApi.UseCases.Orders;
using EventBus;
using EventBus.IntegrationEventInterfaceAbstraction;
using Microsoft.Extensions.Logging;
using OutBoxPattern;
using OutBoxPattern.IntegrationEventLogServices;

namespace BookWooks.OrderApi.Infrastructure.IntegrationEventService;
public class OrderIntegrationEventService : IOrderIntegrationEventService
{
  private readonly IIntegrationEventLogService _eventLogService;
  private readonly IEventBus _messageBus;
  private readonly BookyWooksOrderDbContext _orderDbContext;
  private readonly ILogger<OrderIntegrationEventService> _logger;
  public OrderIntegrationEventService(
        IEventBus eventBus,
        ILogger<OrderIntegrationEventService> logger,
        BookyWooksOrderDbContext orderDbContext,
        IIntegrationEventLogService integrationEventLogService)
  {
    _orderDbContext = orderDbContext ?? throw new ArgumentNullException(nameof(orderDbContext));
    _eventLogService = integrationEventLogService ?? throw new ArgumentNullException(nameof(integrationEventLogService));
    _messageBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      
  }
  public async Task PublishEventsThroughEventBusAsync(Guid transactionId)
  {
    var pendingLogEvents = await _eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId);

    foreach (var logEvt in pendingLogEvents)
    {
      _logger.LogInformation("----- Publishing integration event: {IntegrationEventId} - ({@IntegrationEvent})", logEvt.EventId, logEvt.IntegrationEventBase);

      try
      {
        await _eventLogService.MarkEventAsInProgressAsync(logEvt.EventId);
        await _messageBus.Publish(logEvt.IntegrationEventBase);
        await _eventLogService.MarkEventAsPublishedAsync(logEvt.EventId);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "ERROR publishing integration event: {IntegrationEventId}", logEvt.EventId);

        await _eventLogService.MarkEventAsFailedAsync(logEvt.EventId);
      }
    }
  }

  public async Task AddAndSaveEventAsync(IntegrationEventBase evt)
  {
    _logger.LogInformation("----- Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})", evt.Id, evt);

    var currentTransaction = _orderDbContext.Database.CurrentTransaction;
    if (currentTransaction == null) throw new ArgumentNullException(nameof(currentTransaction));
    await _eventLogService.SaveEventAsync(evt, currentTransaction);
  }
}

