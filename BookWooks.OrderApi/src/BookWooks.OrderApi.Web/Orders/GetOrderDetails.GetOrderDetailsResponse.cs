namespace BookWooks.OrderApi.Web.Orders;

public class GetOrderDetailsResponse
{
  public GetOrderDetailsResponse(Guid id, string status, List<OrderItemRecord> orderItems)
  {
    Id = id;
    Status = status;
    OrderItems = orderItems;
  }

  public Guid Id { get; set; }
  public string Status { get; set; }
  public List<OrderItemRecord> OrderItems { get; set; }
}
