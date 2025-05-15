namespace BookWooks.OrderApi.Web.Orders;
public record GetOrderDetailsResponse(Guid id, string status, List<OrderItemRecord> orderItems);


