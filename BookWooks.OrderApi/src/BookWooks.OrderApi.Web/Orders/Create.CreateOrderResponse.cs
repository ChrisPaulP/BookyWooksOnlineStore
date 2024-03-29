namespace BookWooks.OrderApi.Web.Orders;

public class CreateOrderResponse
{
  public CreateOrderResponse(Guid id, Guid customerId)
  {
    Id = id;
    CustomerId = customerId;
  }
  public Guid Id { get; set; }
  public Guid CustomerId { get; set; }
}
