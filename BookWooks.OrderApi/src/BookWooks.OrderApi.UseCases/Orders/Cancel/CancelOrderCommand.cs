


using BookyWooks.SharedKernel;

namespace BookWooks.OrderApi.UseCases.Cancel;
public record CancelOrderCommand(Guid Id) : ICommand<Result>;


