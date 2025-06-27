using BookyWooks.Messaging.Constants;
using BookyWooks.Messaging.Contracts;
using BookyWooks.Messaging.Messages.InitialMessage;
using Google.Protobuf.WellKnownTypes;

namespace BookWooks.OrderApi.Infrastructure.Common.Processing.Outbox;
internal class ProcessOutboxCommandHandler : ICommandHandler<ProcessOutboxCommand>
{
  private readonly IMediator _mediator;
  private readonly BookyWooksOrderDbContext _dbContext;
  private readonly IDomainEventMapper _domainNotificationsMapper;
  private readonly IMassTransitService _massTransitService;

  public ProcessOutboxCommandHandler(
      IMediator mediator,
      BookyWooksOrderDbContext dbContext,
      IDomainEventMapper domainNotificationsMapper,
      IMassTransitService massTransitService)
  {
    _mediator = mediator;
    _dbContext = dbContext;
    _domainNotificationsMapper = domainNotificationsMapper;
    _massTransitService = massTransitService;
  }

  public async Task Handle(ProcessOutboxCommand command, CancellationToken cancellationToken)
  {
    var messages = await _dbContext.OutboxMessages
                                   .Where(m => m.ProcessedDate == null)
                                   .OrderBy(m => m.OccurredOn)
                                   .ToListAsync(cancellationToken);

    if (messages.Count > 0)
    {
      foreach (var message in messages)
      {
        var type = _domainNotificationsMapper.GetType(message.MessageType);

        var @event = JsonConvert.DeserializeObject(message.Message, type) as  MessageContract;
        if(@event != null)
        {
          using (LogContext.Push(new OutboxMessageContextEnricher(@event)))
          {
            try
            {
              await _massTransitService.Send((dynamic)@event, QueueConstants.CreateOrderMessageQueueName);
              var updatedMessage = message with { ProcessedDate = DateTime.UtcNow };

              // Detach the old tracked entity (optional but safest if EF is already tracking `message`)
              _dbContext.Entry(message).State = EntityState.Detached;

              // Now attach and mark the new version as modified
              _dbContext.OutboxMessages.Update(updatedMessage);
            }
            catch (Exception ex)
            {
              Console.WriteLine(ex);
              throw;
            }
          }
       }
        
      }
      await _dbContext.SaveChangesAsync(cancellationToken);
    }
  }



  private class OutboxMessageContextEnricher : ILogEventEnricher
  {
    //private readonly DomainEventBase _notification;
    private readonly MessageContract _notification;

    //public OutboxMessageContextEnricher(DomainEventBase notification)
    //{
    //  _notification = notification;
    //}
    public OutboxMessageContextEnricher(MessageContract notification)
    {
      _notification = notification;
    }
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
      logEvent.AddOrUpdateProperty(new LogEventProperty("Context", new ScalarValue($"OutboxMessage:{_notification.Id.ToString()}")));
    }
  }
}


// Why Integration Events Are Not Published from ProcessOutboxCommandHandler
// The ProcessOutboxCommandHandler is responsible for processing outbox messages, which are typically domain event notifications.The primary reasons for not publishing integration events directly from this handler are:
// 1.	Separation of Concerns:
//    •	The ProcessOutboxCommandHandler focuses on processing outbox messages and updating their status.It does not handle the logic of transforming domain events into integration events.
// 2.	Event Transformation:
//    •	Domain events are first raised and handled within the same bounded context. If they need to be communicated to other contexts, they are transformed into integration events by specific handlers (like SubscriptionCreatedNotificationHandler).
// 3.	Consistency:
//    •	By handling domain events and publishing integration events in dedicated handlers, the system ensures that each event is processed and published consistently. This approach also makes it easier to manage and debug event flows.
