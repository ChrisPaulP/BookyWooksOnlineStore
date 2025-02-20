namespace BookWooks.OrderApi.UseCases.Orders.List;
public interface IGetOrdersByStatusQueryService
{
   Task<IEnumerable<OrderWithItemsDTO>> GetOrdersByStatusAsync(string status);
}
