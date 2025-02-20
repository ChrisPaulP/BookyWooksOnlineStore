namespace BookyWooks.SharedKernel.DomainEventsDispatching;
public abstract class DomainEventBase : INotification
{
    public Guid Id { get; }
    public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
}