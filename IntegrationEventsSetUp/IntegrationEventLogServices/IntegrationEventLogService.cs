using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Reflection;
using EventBus.IntegrationEventInterfaceAbstraction;

namespace OutBoxPattern.IntegrationEventLogServices;

public class IntegrationEventLogService : IIntegrationEventLogService, IDisposable
{
    private readonly IntegrationEventLogDbContext _integrationEventLogContext;
    private readonly DbConnection _dbConnection;
    private readonly List<Type> _eventTypes;
    private volatile bool _disposedValue;
    private readonly List<Type> r;

    public IntegrationEventLogService(DbConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        _integrationEventLogContext = new IntegrationEventLogDbContext(
            new DbContextOptionsBuilder<IntegrationEventLogDbContext>()
                .UseSqlServer(_dbConnection)
                .Options);
        _eventTypes = new List<Type>();
    }

    public async Task<IEnumerable<IntegrationEventLog>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId)
    {
        var tid = transactionId.ToString();

        var result = await _integrationEventLogContext.IntegrationEventLogs
            .Where(e => e.TransactionId == tid && e.IntegrationEventLogStatus == IntegrationEventLogStatus.NotPublished).ToListAsync();

        foreach (var integrationEventLog in result)
        {
            var eventTypeName = integrationEventLog.EventTypeName;
            var type = Type.GetType("BookWooks.OrderApi.Core.OrderAggregate.IntegrationEvents." + eventTypeName + ", BookWooks.OrderApi.Core");
            _eventTypes.Add(type);
        }

        if (result != null && result.Any())
        { 
            return result.OrderBy(o => o.CreationTime)
                .Select(e => e.DeserializeJsonEventData(_eventTypes.Find(t => t.Name == e.EventTypeShortName)));
        }
        return new List<IntegrationEventLog>();
    }

    public Task SaveEventAsync(IntegrationEventBase @event, IDbContextTransaction transaction)
    {
        //@event.Id = Guid.NewGuid();

        var eventLogEntry = new IntegrationEventLog(@event, transaction.TransactionId);

        _integrationEventLogContext.Database.UseTransaction(transaction.GetDbTransaction());
        _integrationEventLogContext.IntegrationEventLogs.Add(eventLogEntry);

        return _integrationEventLogContext.SaveChangesAsync();
    }

    public Task MarkEventAsPublishedAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, IntegrationEventLogStatus.Published);
    }

    public Task MarkEventAsInProgressAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, IntegrationEventLogStatus.InProgress);
    }

    public Task MarkEventAsFailedAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, IntegrationEventLogStatus.PublishedFailed);
    }

    private Task UpdateEventStatus(Guid eventId, IntegrationEventLogStatus status)
    {
        var eventLogEntry = _integrationEventLogContext.IntegrationEventLogs.Single(ie => ie.EventId == eventId);
        eventLogEntry.IntegrationEventLogStatus = status;

        if (status == IntegrationEventLogStatus.InProgress)
            eventLogEntry.TimesSent++;

        _integrationEventLogContext.IntegrationEventLogs.Update(eventLogEntry);

        return _integrationEventLogContext.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _integrationEventLogContext?.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

