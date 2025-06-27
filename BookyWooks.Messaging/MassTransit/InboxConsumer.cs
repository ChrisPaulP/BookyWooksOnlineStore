using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BookyWooks.SharedKernel.Messages;
using BookyWooks.SharedKernel.Serialization;
using MassTransit.Middleware;
using Newtonsoft.Json;
using BookyWooks.Messaging.Contracts;


namespace BookyWooks.Messaging.MassTransit;
public abstract class InboxConsumer<TMessage, TDbContext> : IConsumer<TMessage>
    where TMessage : MessageContract
    where TDbContext : DbContext, IInboxDbContext
{
    private readonly string _consumerId;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    protected InboxConsumer(IServiceScopeFactory serviceScopeFactory)
    {
        _consumerId = GetType().FullName ?? string.Empty;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var messageId = context.Message.Id;
        var occurredOn = context.Message.OccurredOn;
        var message = JsonConvert.SerializeObject(context.Message, new JsonSerializerSettings
        {
            ContractResolver = new AllPropertiesContractResolver()
        });

        using var scope = _serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<InboxConsumer<TMessage, TDbContext>>>();

        var exists =
            await dbContext.InboxMessages.AnyAsync(x => x.Id == messageId && x.MessageType == _consumerId);

        if (!exists)
        {
            dbContext.InboxMessages.Add(new InboxMessage(messageId, _consumerId, message, occurredOn));
            await dbContext.SaveChangesAsync();
        }

        await using var transactionScope =
            await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);


        //var inboxMessage = await dbContext.InboxMessages
        //.FromSqlRaw("SELECT * FROM InboxMessages WITH (UPDLOCK, READPAST) WHERE Id = {0} AND MessageType = {1}", messageId, _consumerId)
        //.FirstOrDefaultAsync();

        var inboxMessage = await dbContext.InboxMessages
    .Where(x => x.Id == messageId && x.MessageType == _consumerId)
    .FirstOrDefaultAsync();


        if (inboxMessage == null)
        {
            return;
        }

        try
        {
            await Consume(context.Message);
            //inboxMessage.ProcessedDate = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception thrown while consuming message");
            throw;
        }
        finally
        {
            var updatedMessage = inboxMessage with { OccurredOn = DateTime.UtcNow, ProcessedDate = DateTime.UtcNow };
            await dbContext.SaveChangesAsync();
            await transactionScope.CommitAsync();
        }
    }

    public abstract Task Consume(TMessage message);
}
