

namespace BookWooks.OrderApi.Web.Orders;

public class GetOrderByStatusResponse
{
  public GetOrderByStatusResponse(List<OrderRecord> orders, IEnumerable<string>? errors = null)
  {
    Orders = orders;
    Errors = errors ?? Enumerable.Empty<string>();
  }
  public List<OrderRecord> Orders { get;}
  public IEnumerable<string> Errors { get; }
}
