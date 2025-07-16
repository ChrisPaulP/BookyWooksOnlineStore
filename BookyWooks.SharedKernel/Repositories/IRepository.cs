namespace BookyWooks.SharedKernel.Repositories;
public interface IRepository<T> : IRepositoryBase<T>where T : class, IAggregateRoot
{
}
