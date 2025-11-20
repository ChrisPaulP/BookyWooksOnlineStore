using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using MassTransit.EntityFrameworkCoreIntegration;
using SagaOrchestration.Data;

namespace SagaOrchestration.IntegrationTests.TestSetup;

public static class OutboxTestHelpers
{
    public static async Task<OutboxMessage?> WaitForOutboxMessageAsync(IServiceProvider services, TimeSpan timeout, Func<OutboxMessage, bool>? predicate = null)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < timeout)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<StateMachineDbContext>();
            
            var outboxMessages = await context.Set<OutboxMessage>().OrderBy(x => x.SequenceNumber).ToListAsync();
            var found = predicate == null ? outboxMessages.FirstOrDefault() : outboxMessages.FirstOrDefault(predicate);
            if (found != null)
                return found;

            await Task.Delay(150);
        }

        return null;
    }

    public static async Task<OutboxMessage?> WaitForSpecificOutboxMessageAsync<T>(IServiceProvider services, Guid correlationId, TimeSpan timeout) where T : class
    {
        var messageTypeName = typeof(T).Name;
        return await WaitForOutboxMessageAsync(services, timeout, outbox => 
            outbox.CorrelationId == correlationId && 
            outbox.MessageType.Contains(messageTypeName));
    }

    public static async Task<bool> WaitForOutboxDispatchedAsync(IServiceProvider services, Guid outboxId, TimeSpan timeout)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < timeout)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<StateMachineDbContext>();
            
            var state = await context.Set<OutboxState>().FirstOrDefaultAsync(x => x.OutboxId == outboxId);

            if (state != null && state.Delivered != null)
                return true;

            await Task.Delay(150);
        }

        return false;
    }

    public static T? DeserializeOutboxBodyMessage<T>(OutboxMessage outbox) where T : class
    {
        if (string.IsNullOrWhiteSpace(outbox.Body))
            return null;

        var jsonBody = Newtonsoft.Json.Linq.JObject.Parse(outbox.Body);
        var messageToken = jsonBody["message"];
        if (messageToken == null)
            return null;

        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(messageToken.ToString());
    }

    public static async Task<List<OutboxMessage>> GetAllOutboxMessagesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StateMachineDbContext>();
        return await context.Set<OutboxMessage>().OrderBy(x => x.SequenceNumber).ToListAsync();
    }
}
