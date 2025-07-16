namespace BookyWooks.SharedKernel.DomainEventsDispatching;

public class MediatRDomainEventDispatcher<T> : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    public MediatRDomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }
    public async Task<IReadOnlyList<OutboxMessage>> DispatchAndClearEvents(IEnumerable<EntityBase> entitiesWithEvents)
    {
        var domainEvents = new List<DomainEventBase>();
        foreach (var entity in entitiesWithEvents)
        {
            domainEvents.AddRange(entity.DomainEvents);
            entity.ClearDomainEvents();
        }

        var outboxMessages = new List<OutboxMessage>();

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent).ConfigureAwait(false);

            if (domainEvent is IConvertToOutBoxMessage convert)
            {
                var outboxMessage = convert.ToOutboxMessage();
                outboxMessages.Add(outboxMessage);
            }
        }
        return outboxMessages;
    }
}


