namespace BookWooks.OrderApi.UseCases.Errors;
public record FulfillOrderError(string error) : Error(error);

