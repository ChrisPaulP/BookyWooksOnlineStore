namespace BookWooks.OrderApi.UseCases.Errors;
public record OrderNotFound() : Error("Order not found")
{
  public static OrderNotFound Create() => new();
}




