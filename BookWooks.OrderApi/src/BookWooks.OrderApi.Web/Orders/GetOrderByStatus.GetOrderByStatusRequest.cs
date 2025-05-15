namespace BookWooks.OrderApi.Web.Orders;

public class GetOrderByStatusRequest : IRequestWithRoute
{
  public static string Route => "/Orders/{Status}";
  public static string BuildRoute(string status) => Route.Replace("{Status}", status);
  public string Status { get; set; } = "";
}
