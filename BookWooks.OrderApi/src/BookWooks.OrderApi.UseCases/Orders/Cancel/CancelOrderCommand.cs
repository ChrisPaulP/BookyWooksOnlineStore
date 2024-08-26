namespace BookWooks.OrderApi.UseCases.Cancel;
public record CancelOrderCommand(Guid Id) : ICommand<StandardResult>;


