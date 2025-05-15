namespace BookWooks.OrderApi.UseCases.Orders;

public record OrderDTOs(Guid Id, string Status);
public record OrderWithItemsDTO(Guid Id, string Status, IEnumerable<OrderItemDTO> OrderItems);
public record OrderItemDTO(Guid ProductId, string ProductName, string ProductDescription,  decimal Price, int Quantity);
public record OrderCancelledDTO(Guid Id, string Status) : OrderDTOs(Id, Status);
