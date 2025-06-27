using System.Threading.Tasks;
using BookyWooks.SharedKernel.UnitOfWork;

namespace BookyWooks.SharedKernel.Repositories;
public interface IRepositoryBase<T> : IReadRepositoryBase<T> where T : class//, IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }

    Task AddAsync(T entity);
    void AddRange(IEnumerable<T> entities);

    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
}
