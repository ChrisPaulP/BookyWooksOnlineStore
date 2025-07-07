namespace BookWooks.OrderApi.Infrastructure.Common.Processing.Outbox;

public class OutboxMethods : IOutbox
{
  private readonly IOutboxDbContext _dbContext;

  public OutboxMethods(IOutboxDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public void Add(OutboxMessage message)
  {
    _dbContext.OutboxMessages.Add(message);
    // Do not call SaveChanges here; let the main transaction handle it
  }

  public Task Save()
  {
    return Task.CompletedTask;
  }
}
