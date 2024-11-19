namespace BookWooks.OrderApi.Web.Orders;

public class CreateOrderResponse
{
  public CreateOrderResponse(Guid id, Guid customerId, IEnumerable<string>? errors = null)
  {
    Id = id;
    CustomerId = customerId;
    Errors = errors ?? Enumerable.Empty<string>();
  }
  public Guid Id { get;}
  public Guid CustomerId { get;}
  public IEnumerable<string> Errors { get;}
}

