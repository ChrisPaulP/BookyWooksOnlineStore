using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.SharedKernel.DomainEventsDispatching;

public class DomainEventsDispatcherNotificationHandlerDecorator<T> : INotificationHandler<T>
        where T : INotification
{
    private readonly INotificationHandler<T> _decorated;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly DbContext _dbContext;

    public DomainEventsDispatcherNotificationHandlerDecorator(
        IDomainEventDispatcher domainEventDispatcher,
        INotificationHandler<T> decorated,
        DbContext dbContext)
    {
        _domainEventDispatcher = domainEventDispatcher;
        _decorated = decorated;
        _dbContext = dbContext;
    }

    public async Task Handle(T notification, CancellationToken cancellationToken)
    {
        await _decorated.Handle(notification, cancellationToken);

        var entitiesWithEvents = _dbContext.ChangeTracker.Entries<EntityBase>()
          .Where(e => e.Entity.DomainEvents.Any())
          .Select(e => e.Entity)
          .ToArray();

        await _domainEventDispatcher.DispatchAndClearEvents(entitiesWithEvents);
    }
}
