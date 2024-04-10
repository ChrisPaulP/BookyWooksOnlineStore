namespace BookWooks.OrderApi.Infrastructure.Data.Repositories;
public class Repository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T>
        where T : class, IAggregateRoot
{
  public Repository(BookyWooksOrderDbContext context) : base(context)
  {

  }
}

