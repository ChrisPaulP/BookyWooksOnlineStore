//namespace BookWooks.OrderApi.Infrastructure.Data.Repositories.Abstract;
//public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class, IAggregateRoot
//{
//  private readonly BookyWooksOrderDbContext _orderDbContext;
//  private readonly DbSet<T> _entitySet;
//  public RepositoryBase(BookyWooksOrderDbContext orderDbContext)
//  {
//    _orderDbContext = orderDbContext;
//    _entitySet = _orderDbContext.Set<T>();
//  }
//  public IUnitOfWork UnitOfWork => _orderDbContext;

//  public virtual async Task AddAsync(T entity) => await ExecuteWithErrorHandlingAsync(async () => await _entitySet.AddAsync(entity));
//  public virtual void AddRange(IEnumerable<T> entities) => _entitySet.AddRange(entities);

//  public virtual Task UpdateAsync(T entity)
//  {
//    _entitySet.Attach(entity);
//    _orderDbContext.Entry(entity).State = EntityState.Modified;
//    return Task.CompletedTask;
//  }
//  public virtual void Remove(T entity) => _entitySet.Remove(entity);
//  public virtual void RemoveRange(IEnumerable<T> entities) => _entitySet.RemoveRange(entities);
//  public virtual bool Contains(ISpecification<T> specification) => Count(specification) > 0 ? true : false;
//  public virtual bool Contains(Expression<Func<T, bool>> predicate) => Count(predicate) > 0 ? true : false;
//  public virtual int Count(ISpecification<T> specification) => ApplySpecification(specification).Count();
//  public virtual int Count(Expression<Func<T, bool>> predicate) => _entitySet.Where(predicate).Count();
//  public IQueryable<T> ApplySpecification(ISpecification<T> spec) => SpecificationEvaluator<T>.GetQuery(_entitySet.AsQueryable(), spec);
//  public virtual async Task<T?> GetByIdAsync(Guid id) => await ExecuteWithErrorHandlingAsync(async () => await _entitySet.FindAsync(id));
//  public virtual async Task<List<T>> FindAllAsync(ISpecification<T> specification) => await ExecuteWithErrorHandlingAsync(async () => await ApplySpecification(specification).AsNoTracking().ToListAsync());

//  public virtual async Task<T?> FindAsync(ISpecification<T> specification) => await ExecuteWithErrorHandlingAsync(async () => await ApplySpecification(specification).AsNoTracking().FirstOrDefaultAsync());
//  public virtual async Task<List<T>> ListAllAsync() => await ExecuteWithErrorHandlingAsync(async () => await _entitySet.AsNoTracking().ToListAsync());

//  protected async Task<TResult> ExecuteWithErrorHandlingAsync<TResult>(Func<Task<TResult>> action)
//  {
//    try
//    {
//      return await action();
//    }
//    catch (Exception ex)
//    {
//      Console.WriteLine(ex);
//      throw;
//    }
//  }
//}



using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookWooks.OrderApi.Infrastructure.Data.Repositories.Abstract;

public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class, IAggregateRoot
{
  private readonly BookyWooksOrderDbContext _orderDbContext;
  private readonly DbSet<T> _entitySet;
  private readonly ILogger<RepositoryBase<T>>? _logger;

  protected RepositoryBase(BookyWooksOrderDbContext orderDbContext, ILogger<RepositoryBase<T>>? logger = null)
  {
    _orderDbContext = orderDbContext;
    _entitySet = _orderDbContext.Set<T>();
    _logger = logger;
  }

  public IUnitOfWork UnitOfWork => _orderDbContext;

  // -------------------------------
  // Write Operations
  // -------------------------------

  public virtual async Task AddAsync(T entity) =>
      await ExecuteWithErrorHandlingAsync(() => _entitySet.AddAsync(entity).AsTask());

  public virtual void AddRange(IEnumerable<T> entities) =>
      ExecuteWithErrorHandling(() => _entitySet.AddRange(entities));

    public virtual Task UpdateAsync(T entity)
    {
        return ExecuteWithErrorHandlingAsync(async () =>
        {
            _entitySet.Attach(entity);
            _orderDbContext.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
            return true;
        });
    }

  public virtual void Remove(T entity) =>
      ExecuteWithErrorHandling(() => _entitySet.Remove(entity));

  public virtual void RemoveRange(IEnumerable<T> entities) =>
      ExecuteWithErrorHandling(() => _entitySet.RemoveRange(entities));

  // -------------------------------
  // Read Operations
  // -------------------------------

  public virtual bool Contains(ISpecification<T> specification) =>
      Count(specification) > 0;

  public virtual bool Contains(Expression<Func<T, bool>> predicate) =>
      Count(predicate) > 0;

  public virtual int Count(ISpecification<T> specification) =>
      ApplySpecification(specification).Count();

  public virtual int Count(Expression<Func<T, bool>> predicate) =>
      _entitySet.Count(predicate);

  public IQueryable<T> ApplySpecification(ISpecification<T> spec) =>
      SpecificationEvaluator<T>.GetQuery(_entitySet.AsQueryable(), spec);

  public virtual async Task<T?> GetByIdAsync(Guid id) =>
      await ExecuteWithErrorHandlingAsync(() => _entitySet.FindAsync(id).AsTask());

  public virtual async Task<List<T>> FindAllAsync(ISpecification<T> specification) =>
      await ExecuteQueryToListAsync(ApplySpecification(specification).AsNoTracking());

  public virtual async Task<T?> FindAsync(ISpecification<T> specification) =>
      await ExecuteQueryFirstOrDefaultAsync(ApplySpecification(specification).AsNoTracking());

  public virtual async Task<List<T>> ListAllAsync() =>
      await ExecuteQueryToListAsync(_entitySet.AsNoTracking());

  // -------------------------------
  // Internal Helpers
  // -------------------------------

  protected async Task<List<T>> ExecuteQueryToListAsync(IQueryable<T> queryable) =>
      await ExecuteWithErrorHandlingAsync(() => queryable.ToListAsync());

  protected async Task<T?> ExecuteQueryFirstOrDefaultAsync(IQueryable<T> queryable) =>
      await ExecuteWithErrorHandlingAsync(() => queryable.FirstOrDefaultAsync());

  protected async Task<TResult> ExecuteWithErrorHandlingAsync<TResult>(Func<Task<TResult>> action, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
  {
    try
    {
      return await action();
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Error in async repository method {Method}", methodName);
      throw;
    }
  }
  protected void ExecuteWithErrorHandling(Action action, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
  {
    try
    {
      action();
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Error in sync repository method {Method}", methodName);
      throw;
    }
  }
}
