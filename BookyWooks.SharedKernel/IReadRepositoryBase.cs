namespace BookyWooks.SharedKernel;
public interface IReadRepositoryBase<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> FindAllAsync(ISpecification<T> specification);
    Task<T?> FindAsync(ISpecification<T> specification);
    bool Contains(ISpecification<T> specification);
    bool Contains(Expression<Func<T, bool>> predicate);
    int Count(ISpecification<T> specification);
    int Count(Expression<Func<T, bool>> predicate);
    Task<List<T>> ListAllAsync();
}
