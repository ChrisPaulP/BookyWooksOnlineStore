

namespace BookWooks.OrderApi.UseCases.Orders.List;
public interface IGetOrdersByStatusQueryService
{
  //Task<IEnumerable<OrderDTO>> ListOrdersAsync();
  Task<IEnumerable<OrderDTO>> GetOrdersByStatusAsync(string status);
}
