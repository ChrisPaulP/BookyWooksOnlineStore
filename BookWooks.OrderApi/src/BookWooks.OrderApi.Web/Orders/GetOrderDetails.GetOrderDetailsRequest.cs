namespace BookWooks.OrderApi.Web.Orders;

public class GetOrderDetailsRequest : IRequestWithRoute
{ 
  public static string Route => "/Orders/{OrderId:guid}";
  public static string BuildRoute(Guid orderId) => Route.Replace("{OrderId:guid}", orderId.ToString());
  public Guid OrderId { get; set; }
}
