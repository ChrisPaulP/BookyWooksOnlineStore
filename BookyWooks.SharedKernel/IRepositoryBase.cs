using System.Threading.Tasks;

namespace BookyWooks.SharedKernel;
public interface IRepositoryBase<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }

    Task AddAsync(T entity);
    void AddRange(IEnumerable<T> entities);

    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    void Update(T entity);
}
