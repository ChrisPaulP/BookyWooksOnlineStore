namespace BookWooks.OrderApi.Web.Orders;

public class ListOrdersForCustomerRequest
{
  public const string Route = "/OrdersByCustomer/{CustomerId:int}";
  public static string BuildRoute(Guid customerId) => Route.Replace("{CustomerId:guid}", customerId.ToString());

  public Guid CustomerId { get; set; }
}
