namespace BookWooks.OrderApi.Web.Orders;

public class GetOrdersResponse
{
  public List<OrderRecord> Orders { get; set; } = new List<OrderRecord>();
}
