namespace BookWooks.OrderApi.Web.Orders;

public class ListOrdersForCustomerRequest
{
  public const string Route = "/OrdersByCustomer/{CustomerId:int}";
  public static string BuildRoute(int customerId) => Route.Replace("{CustomerId:int}", customerId.ToString());

  public int CustomerId { get; set; }
}
