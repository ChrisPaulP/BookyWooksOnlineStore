namespace BookWooks.OrderApi.Web.Orders;

public record OrderRecord(Guid Id, string Status, IEnumerable<OrderItemRecord>? OrderItems);
public record OrderItemRecord(int BookId, string BookTitle, decimal BookPrice, int Quantity);
