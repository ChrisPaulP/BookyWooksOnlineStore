namespace BookWooks.OrderApi.Web.Orders;

public record OrderRecord(Guid Id, string Status, IEnumerable<OrderItemRecord> OrderItems);
public record OrderItemRecord(Guid ProductId, decimal Price, int Quantity);
