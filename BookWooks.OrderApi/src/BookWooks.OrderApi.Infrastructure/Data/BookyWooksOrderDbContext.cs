using System.Data;
using System.Reflection;

using BookWooks.OrderApi.Core.OrderAggregate;

using BookyWooks.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookWooks.OrderApi.Infrastructure.Data;
public class BookyWooksOrderDbContext : DbContext , IUnitOfWork//, IBookyWooksOrderDbContext    
{
  private readonly IDomainEventDispatcher<Guid>? _dispatcher;
  public IDbContextTransaction? CurrentTransaction { get; private set; }
  public bool HasActiveTransaction => CurrentTransaction != null;

  public BookyWooksOrderDbContext(DbContextOptions<BookyWooksOrderDbContext> options,
    IDomainEventDispatcher<Guid>? dispatcher)
      : base(options)
  {
    _dispatcher = dispatcher;
  }


  public DbSet<Order> Orders  { get; set; }
  public DbSet<OrderItem> OrderItems { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    // Call base method at the end
    base.OnModelCreating(modelBuilder);
  }

  public  async Task<int> SaveEntitiesAsync(CancellationToken cancellationToken = new CancellationToken())
  {
    var initialTrackedEntities = ChangeTracker.Entries().ToList();
    int result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);


    // ignore events if no dispatcher provided
    if (_dispatcher == null) return result;
  

    // dispatch events only if save was successful
    var entitiesWithEvents = ChangeTracker.Entries<EntityBase<Guid>>()
        .Select(e => e.Entity)
        .Where(e => e.DomainEvents.Any())
        .ToArray();

    await _dispatcher.DispatchAndClearEvents(entitiesWithEvents);

    return result;
  }

  public override int SaveChanges()
  {
    return SaveChangesAsync().GetAwaiter().GetResult();
  }

  public async Task<IDbContextTransaction?> BeginTransactionAsync()
  {
    if (CurrentTransaction != null) return null;

    CurrentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

    return CurrentTransaction;
  }
  public async Task CommitTransactionAsync(IDbContextTransaction transaction)
  {
    if (transaction == null) throw new ArgumentNullException(nameof(transaction));
    if (transaction != CurrentTransaction) throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

    try
    {
      await SaveChangesAsync();
      transaction.Commit();
    }
    catch
    {
      RollbackTransaction();
      throw;
    }
    finally
    {
      if (CurrentTransaction != null)
      {
        CurrentTransaction.Dispose();
        CurrentTransaction = null;
      }
    }
  }
  public void RollbackTransaction()
  {
    try
    {
      CurrentTransaction?.Rollback();
    }
    finally
    {
      if (CurrentTransaction != null)
      {
        CurrentTransaction.Dispose();
        CurrentTransaction = null;
      }
    }
  }

}
