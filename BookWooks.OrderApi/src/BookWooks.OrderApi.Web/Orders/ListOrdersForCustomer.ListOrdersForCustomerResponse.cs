namespace BookWooks.OrderApi.Web.Orders;

public class ListOrdersForCustomerResponse 
{
  public List<OrderRecord> Orders { get; set; } = new List<OrderRecord>();
}

