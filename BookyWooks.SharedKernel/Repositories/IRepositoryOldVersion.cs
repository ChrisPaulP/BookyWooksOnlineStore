using BookyWooks.SharedKernel.Specification;
using BookyWooks.SharedKernel.UnitOfWork;

namespace BookyWooks.SharedKernel.Repositories;

public interface IRepositoryOldVersion<T, Tkey> where T : IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
    Task<T?> TFindById(Tkey id);
    Task<List<T>> FindAsync(ISpecification<T> specification);

    Task<T> AddAsync(T entity);
    void AddRange(IEnumerable<T> entities);

    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);

    void Update(T entity);

    bool Contains(ISpecification<T> specification);
    bool Contains(Expression<Func<T, bool>> predicate);

    int Count(ISpecification<T> specification);
    int Count(Expression<Func<T, bool>> predicate);
}
