﻿using BookyWooks.SharedKernel.Messages;

namespace BookyWooks.SharedKernel.DomainEventsDispatching;
/// <summary>
/// A simple interface for sending domain events. Can use MediatR or any other implementation.
/// </summary>
public interface IDomainEventDispatcher
{
    //Task DispatchAndClearEvents(IEnumerable<EntityBase> entitiesWithEvents);
    Task<IReadOnlyList<OutboxMessage>> DispatchAndClearEvents(IEnumerable<EntityBase> entitiesWithEvents);
}