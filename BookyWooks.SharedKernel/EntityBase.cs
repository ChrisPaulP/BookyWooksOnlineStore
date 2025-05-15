using BookyWooks.SharedKernel.DomainEventsDispatching;

namespace BookyWooks.SharedKernel;

public abstract record EntityBase
{

    private List<DomainEventBase> _domainEvents = new();
    [NotMapped]
    public IEnumerable<DomainEventBase> DomainEvents => _domainEvents.AsReadOnly();

    public void RegisterDomainEvent(DomainEventBase domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

//public abstract record EntityBase(Guid Id, ImmutableList<DomainEventBase> DomainEvents)
//{
//    protected EntityBase() : this(Guid.NewGuid(), ImmutableList<DomainEventBase>.Empty) { }

//    protected EntityBase(Guid id) : this(id, ImmutableList<DomainEventBase>.Empty) { }

//    public EntityBase RegisterDomainEvent(DomainEventBase domainEvent) =>
//        this with { DomainEvents = DomainEvents.Add(domainEvent) };

//    public EntityBase ClearDomainEvents() =>
//        this with { DomainEvents = ImmutableList<DomainEventBase>.Empty };
//}