namespace BookWooks.OrderApi.Web.Orders;

public class CancelOrderResponse
{
  public string Message { get; }

  public CancelOrderResponse()
  {
    Message = "Order cancelled.";
  }
}
