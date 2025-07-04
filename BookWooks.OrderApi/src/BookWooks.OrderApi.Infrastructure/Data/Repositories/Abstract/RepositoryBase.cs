﻿using BookyWooks.SharedKernel.Repositories;
using BookyWooks.SharedKernel.Specification;
using BookyWooks.SharedKernel.UnitOfWork;
using LanguageExt;

namespace BookWooks.OrderApi.Infrastructure.Data.Repositories.Abstract;
public abstract class RepositoryBase<T> : IRepositoryBase<T>
       where T : class//, IAggregateRoot
{
  private readonly BookyWooksOrderDbContext _orderDbContext;
  private readonly DbSet<T> _entitySet;
  public RepositoryBase(BookyWooksOrderDbContext orderDbContext)
  {
    _orderDbContext = orderDbContext;
    _entitySet = _orderDbContext.Set<T>();
  }
  public IUnitOfWork UnitOfWork => _orderDbContext;

  public virtual async Task AddAsync(T entity)
  {
     await _entitySet.AddAsync(entity);
  }

  public virtual void AddRange(IEnumerable<T> entities)
  {
    _entitySet.AddRange(entities);
  }

  public virtual void Remove(T entity)
  {
    _entitySet.Remove(entity);
    //_orderDbContext.Set<T>().Remove(entity);
  }

  public virtual void RemoveRange(IEnumerable<T> entities)
  {
    _entitySet.RemoveRange(entities);
  }

  public virtual bool Contains(ISpecification<T> specification)
  {
    return Count(specification) > 0 ? true : false;
  }

  public virtual bool Contains(Expression<Func<T, bool>> predicate)
  {
    return Count(predicate) > 0 ? true : false;
  }

  public virtual int Count(ISpecification<T> specification)
  {
    return ApplySpecification(specification).Count();
  }

  public virtual int Count(Expression<Func<T, bool>> predicate)
  {
    return _entitySet.Where(predicate).Count();
  }
  public IQueryable<T> ApplySpecification(ISpecification<T> spec)
  {
    return SpecificationEvaluator<T>.GetQuery(_entitySet.AsQueryable(), spec);
  }

  public virtual async Task<T?> GetByIdAsync(Guid id)
  {
    return await _entitySet.FindAsync(id);
  }

  public virtual async Task<List<T>> FindAllAsync(ISpecification<T> specification)
  {
    return  await ApplySpecification(specification).ToListAsync();
  }
  public virtual async Task<T?> FindAsync(ISpecification<T> specification)
  {
    return await ApplySpecification(specification).FirstOrDefaultAsync();
  }
  public virtual Task UpdateAsync(T entity)
  {
    _entitySet.Entry(entity).State = EntityState.Modified;
    return Task.CompletedTask;
  }
  public virtual async Task<List<T>> ListAllAsync()
  {
    return await _entitySet.ToListAsync();
  }
}



