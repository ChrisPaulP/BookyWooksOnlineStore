namespace BookWooks.OrderApi.UseCases.Orders;
internal static class OrderMappingExtensions
{
  public static OrderDTO ToOrderDTO(this Order order) => new OrderDTO(order.Id,order.Status.Label);
}
