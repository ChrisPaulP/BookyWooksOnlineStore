namespace BookWooks.OrderApi.Infrastructure.Common.Processing.Outbox;
//public class OutboxMethods : IOutbox
//{
//  private readonly IBookyWooksOrderDbContextFactory _dbContextFactory;

//  public OutboxMethods(IBookyWooksOrderDbContextFactory outboxContext)
//  {
//    _dbContextFactory = outboxContext;
//  }

//  public void Add(OutboxMessage message)
//  {
//    using var outboxContext = _dbContextFactory.CreateDbContext();
//    outboxContext.OutboxMessages.Add(message);
//    //outboxContext.SaveChanges(); might need to add this
//  }

//  public Task Save()
//  {
//    return Task.CompletedTask; // Save is done automatically using EF Core Change Tracking mechanism during SaveChanges.
//  }
//}
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
