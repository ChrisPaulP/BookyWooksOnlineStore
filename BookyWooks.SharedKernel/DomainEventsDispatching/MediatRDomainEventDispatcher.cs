using BookyWooks.SharedKernel.Messages;
using BookyWooks.SharedKernel.Serialization;

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

            //if (domainEvent is IConvertToOutBoxMessage convertToOutBoxMessage)
            //{
            //    var outboxMessage = new OutboxMessage(
            //    domainEvent.Id,
            //    domainEvent.GetType().Name,
            //    JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings
            //    {
            //        ContractResolver = new AllPropertiesContractResolver()
            //    }),
            //    domainEvent.DateOccurred);

            //    outboxMessages.Add(outboxMessage);
            //}
            if (domainEvent is IConvert convert)
            {
                var outboxMessage = convert.ToOutboxMessage();
                outboxMessages.Add(outboxMessage);
            }
        }

        return outboxMessages;
    }
    //public async Task DispatchAndClearEvents(IEnumerable<EntityBase> entitiesWithEvents)
    //{
    //    var domainEvents = new List<DomainEventBase>();
    //    foreach (var entity in entitiesWithEvents)
    //    {
    //        domainEvents.AddRange(entity.DomainEvents);
    //        entity.ClearDomainEvents();
    //    }

    //    foreach (var domainEvent in domainEvents)
    //    {
    //        await _mediator.Publish(domainEvent).ConfigureAwait(false);

    //        var outboxMessage = new OutboxMessage(
    //            domainEvent.Id,
    //            domainEvent.GetType().Name,
    //            JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings
    //            {
    //                ContractResolver = new AllPropertiesContractResolver()
    //            }),
    //            domainEvent.DateOccurred);

    //        _outbox.Add(outboxMessage);
    //    }
    //}
}


