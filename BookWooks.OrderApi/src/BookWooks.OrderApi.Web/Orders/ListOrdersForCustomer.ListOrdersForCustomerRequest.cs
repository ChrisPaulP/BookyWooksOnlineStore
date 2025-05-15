namespace BookWooks.OrderApi.Web.Orders;

public class ListOrdersForCustomerRequest : IRequestWithRoute
{
  public static string Route => "/OrdersByCustomer/{CustomerId:int}";

  public Guid CustomerId { get; set; }
}
