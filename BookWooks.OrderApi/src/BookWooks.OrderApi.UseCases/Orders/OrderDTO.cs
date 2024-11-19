namespace BookWooks.OrderApi.UseCases.Orders;

public record OrderDTO(Guid Id, string Status, IEnumerable<OrderItemDTO> OrderItems);
public record OrderItemDTO(Guid ProductId,  decimal Price, int Quantity);
