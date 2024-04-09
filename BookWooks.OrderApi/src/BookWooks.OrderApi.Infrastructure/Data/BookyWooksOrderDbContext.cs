using System.Data;
using System.Reflection;
using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookyWooks.SharedKernel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookWooks.OrderApi.Infrastructure.Data;
public class BookyWooksOrderDbContext : DbContext , IUnitOfWork//, IBookyWooksOrderDbContext    
{
  private readonly IDomainEventDispatcher? _dispatcher;
  public IDbContextTransaction? CurrentTransaction { get; private set; }
  public bool HasActiveTransaction => CurrentTransaction != null;

  public BookyWooksOrderDbContext(DbContextOptions<BookyWooksOrderDbContext> options,
    IDomainEventDispatcher? dispatcher)
      : base(options)
  {
    _dispatcher = dispatcher;
  }


  public DbSet<Order> Orders  { get; set; }
  public DbSet<OrderItem> OrderItems { get; set; }
  public DbSet<Customer> Customers { get; set; }
  public DbSet<Product> Products { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    // Call base method at the end
    base.OnModelCreating(modelBuilder);


    modelBuilder.AddInboxStateEntity();
    modelBuilder.AddOutboxMessageEntity();
    modelBuilder.AddOutboxStateEntity();
  }
  public async Task<int> SaveEntitiesAsync(CancellationToken cancellationToken = default)
  {
    // ignore events if no dispatcher provided
    if (_dispatcher == null)
      return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    // Retrieve entities with domain events
    var entitiesWithEvents = ChangeTracker.Entries<EntityBase>()
        .Where(e => e.Entity.DomainEvents.Any())
        .Select(e => e.Entity)
        .ToArray();

    // Dispatch events only if there are entities with domain events
    if (entitiesWithEvents.Length > 0)
      await _dispatcher.DispatchAndClearEvents(entitiesWithEvents).ConfigureAwait(false);

    // Save changes to the database
    return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
