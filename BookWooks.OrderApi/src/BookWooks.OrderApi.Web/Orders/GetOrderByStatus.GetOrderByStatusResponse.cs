

namespace BookWooks.OrderApi.Web.Orders;

public class GetOrderByStatusResponse
{
  public List<OrderRecord> Orders { get; set; } = new List<OrderRecord>();
}
