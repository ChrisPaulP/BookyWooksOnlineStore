namespace BookWooks.OrderApi.Web.Orders;

public class CreateOrderResponse
{
  public CreateOrderResponse(Guid id, string userName)
  {
    Id = id;
    UserName = userName;
  }
  public Guid Id { get; set; }
  public string UserName { get; set; }
}
