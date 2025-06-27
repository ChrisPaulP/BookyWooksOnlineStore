namespace BookyWooks.SharedKernel.DomainEventsDispatching;
public abstract class DomainEventBase : INotification
{
    public Guid Id { get; }
    public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
    public bool IsAlsoIntegrationEvent { get; set; }
    protected DomainEventBase() : this(Guid.NewGuid()) { }

    protected DomainEventBase(Guid id)
    {
        Id = id;
    }
}
