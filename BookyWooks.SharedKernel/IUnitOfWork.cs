namespace BookyWooks.SharedKernel;
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    Task<int> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken));
}
