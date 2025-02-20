using BookyWooks.SharedKernel.Messages;
using BookyWooks.SharedKernel.Serialization;

namespace BookyWooks.SharedKernel.DomainEventsDispatching;

public class MediatRDomainEventDispatcher<T> : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly IOutbox _outbox;

    public MediatRDomainEventDispatcher(IMediator mediator, IOutbox outbox)
    {
        _mediator = mediator;
        _outbox = outbox;
    }

    public async Task DispatchAndClearEvents(IEnumerable<EntityBase> entitiesWithEvents)
    {
        var domainEvents = new List<DomainEventBase>();
        foreach (var entity in entitiesWithEvents)
        {
            domainEvents.AddRange(entity.DomainEvents);
            entity.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent).ConfigureAwait(false);

            var outboxMessage = new OutboxMessage(
                domainEvent.Id,
                domainEvent.GetType().Name,
                JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings
                {
                    ContractResolver = new AllPropertiesContractResolver()
                }),
                domainEvent.DateOccurred);

            _outbox.Add(outboxMessage);
        }
    }
}


