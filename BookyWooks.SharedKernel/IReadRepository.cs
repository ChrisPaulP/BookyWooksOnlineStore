namespace BookyWooks.SharedKernel;
public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot { }

