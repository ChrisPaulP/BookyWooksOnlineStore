namespace BookWooks.OrderApi.Web.Orders;

public record CancelOrderRequest(Guid OrderId) : IRequestWithRoute
{
  public static string Route => "/Orders/Cancel/{OrderId:guid}";
}
