namespace BookWooks.OrderApi.Infrastructure.Common.Processing.Outbox;
public class OutboxMethods : IOutbox
{
  private readonly BookyWooksOrderDbContext _outboxContext;

  public OutboxMethods(BookyWooksOrderDbContext outboxContext)
  {
    _outboxContext = outboxContext;
  }

  public void Add(OutboxMessage message)
  {
    _outboxContext.OutboxMessages.Add(message);
  }

  public Task Save()
  {
    return Task.CompletedTask; // Save is done automatically using EF Core Change Tracking mechanism during SaveChanges.
  }
}
