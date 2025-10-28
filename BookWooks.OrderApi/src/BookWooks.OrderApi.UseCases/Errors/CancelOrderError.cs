namespace BookWooks.OrderApi.UseCases.Errors;
public record CancelOrderError(string error) : Error(error);

