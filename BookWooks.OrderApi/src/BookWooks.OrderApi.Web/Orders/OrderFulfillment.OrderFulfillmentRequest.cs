namespace BookWooks.OrderApi.Web.Orders;

public record OrderFulfillmentRequest(Guid OrderId) : IRequestWithRoute
{
  public static string Route => "/Orders/{Order:guid}/OrderFulfilled";
}
