namespace BookWooks.OrderApi.Web.Orders;
public record OrderRecord(Guid Id, string Status);
public record OrderWithItemsRecord(Guid Id, string Status, IEnumerable<OrderItemRecord> OrderItems);
public record OrderItemRecord(Guid ProductId, string ProductName, string ProductDescription, decimal Price, int Quantity);
