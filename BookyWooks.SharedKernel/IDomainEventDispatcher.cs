namespace BookyWooks.SharedKernel;
/// <summary>
/// A simple interface for sending domain events. Can use MediatR or any other implementation.
/// </summary>
public interface IDomainEventDispatcher <T> 
{
    Task DispatchAndClearEvents(IEnumerable<EntityBase<T>> entitiesWithEvents);
}