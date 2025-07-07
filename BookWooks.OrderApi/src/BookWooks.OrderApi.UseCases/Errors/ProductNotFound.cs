namespace BookWooks.OrderApi.UseCases.Errors;
public record ProductNotFound() : Error("Product not found")
{
  public static ProductNotFound Create() => new();
}

