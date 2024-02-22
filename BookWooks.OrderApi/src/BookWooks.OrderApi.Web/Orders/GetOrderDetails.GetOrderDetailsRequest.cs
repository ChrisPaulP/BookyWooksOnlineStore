namespace BookWooks.OrderApi.Web.Orders;

public class GetOrderDetailsRequest
{
  public const string Route = "/Orders/{OrderId:guid}";
  public static string BuildRoute(Guid orderId) => Route.Replace("{OrderId:guid}", orderId.ToString());

  public Guid OrderId { get; set; }
}
