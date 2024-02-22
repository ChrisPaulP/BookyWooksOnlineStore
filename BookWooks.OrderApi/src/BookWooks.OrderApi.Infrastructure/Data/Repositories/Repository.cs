using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.Infrastructure.Data.Repositories.Abstract;
using BookyWooks.SharedKernel;

namespace BookWooks.OrderApi.Infrastructure.Data.Repositories;
public class Repository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T>
        where T : class, IAggregateRoot
{
  public Repository(BookyWooksOrderDbContext context) : base(context)
  {

  }
}

