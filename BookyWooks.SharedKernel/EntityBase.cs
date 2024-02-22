namespace BookyWooks.SharedKernel;
public abstract class EntityBase<T> 
{
    public T Id { get; set; }

    private List<DomainEventBase> _domainEvents = new();
    [NotMapped]
    public IEnumerable<DomainEventBase> DomainEvents => _domainEvents.AsReadOnly();

    public void RegisterDomainEvent(DomainEventBase domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}