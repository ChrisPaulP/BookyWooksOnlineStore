using BookyWooks.SharedKernel.Commands;
using BookyWooks.SharedKernel.ResultPattern;

namespace BookWooks.OrderApi.UseCases.Cancel;
public record CancelOrderCommand(Guid Id) : ICommand<StandardResult>;


