namespace BookyWooks.SharedKernel;
public class MediatRDomainEventDispatcher<T> : IDomainEventDispatcher<T> 
{
    private readonly IMediator _mediator;

    public MediatRDomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }
    public async Task DispatchAndClearEvents(IEnumerable<EntityBase<T>> entitiesWithEvents)
    {
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToArray();
            entity.ClearDomainEvents();
            foreach (var domainEvent in events)
            {
                await _mediator.Publish(domainEvent).ConfigureAwait(false);
            }
        }
    }
}

