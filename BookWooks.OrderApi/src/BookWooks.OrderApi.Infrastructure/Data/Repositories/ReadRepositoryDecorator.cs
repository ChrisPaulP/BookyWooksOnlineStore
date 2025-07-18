﻿namespace BookWooks.OrderApi.Infrastructure.Data.Repositories;
public class ReadRepositoryDecorator<T> : RepositoryBase<T>, IReadRepository<T>
      where T : class , IAggregateRoot
{
  private readonly BookyWooksOrderDbContext _orderDbContext;
  private readonly ICacheService _cacheService;
  private readonly DbSet<T> _entitySet;
  public ReadRepositoryDecorator(BookyWooksOrderDbContext orderDbContext, ICacheService cacheService) : base(orderDbContext)
  {
    _orderDbContext = orderDbContext;
    _entitySet = _orderDbContext.Set<T>();
    _cacheService = cacheService;
  }
  //public override bool Contains(ISpecification<T> specification)
  //{
  //  return Count(specification) > 0 ? true : false;
  //}


  public override int Count(Expression<Func<T, bool>> predicate)
  {
    //return _readRepository.Count(predicate);

    var cacheKey = "cachekey"; // or specification.CacheKey if needed

    var result = _cacheService.GetOrSet(
    cacheKey,
        () => _entitySet.Where(predicate).Count(), TimeSpan.FromDays(1));

    return result;
  }
  public override async Task<List<T>> FindAllAsync(ISpecification<T> specification)
  {
    var cacheKey = specification.CacheKey ?? String.Empty;

    var result = await _cacheService.GetOrSetAsync(
        cacheKey,
        () => ApplySpecification(specification).AsNoTracking().ToListAsync());


    return result ?? new List<T>();
  }


  public override Task<T?> FindAsync(ISpecification<T> specification)
  {
    var cacheKey = specification.CacheKey ?? "CacheKey"; // if needed

    var result = _cacheService.GetOrSetAsync(
        cacheKey,
        () => ApplySpecification(specification).FirstOrDefaultAsync());

    return result;
  }
}
