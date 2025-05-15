namespace BookWooks.OrderApi.UseCases.Orders;
public static class OrderMappingExtensions
{
  public static OrderDTOs ToOrderDTO(this Order order) => new OrderDTOs(order.OrderId.Value, order.Status.Label);
  public static OrderWithItemsDTO ToOrderWithItemsDTO(this Order order) =>
          new OrderWithItemsDTO(
              order.OrderId.Value,
              order.Status.Label,
              order.OrderItems.Select(item => new OrderItemDTO(
              item.ProductId.Value,
              item.ProductName.Value,
              item.ProductDescription.Value,
              item.Price.Value,
              item.Quantity.Value)).ToList()
          );
  public static OrderCancelledDTO ToOrderCancelledDTO(this Order order) => new OrderCancelledDTO(order.OrderId.Value, order.Status.Label);
}
