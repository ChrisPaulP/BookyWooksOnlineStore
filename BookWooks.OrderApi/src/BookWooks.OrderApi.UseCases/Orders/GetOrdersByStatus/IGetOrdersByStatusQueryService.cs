namespace BookWooks.OrderApi.UseCases.Orders.List;
public interface IGetOrdersByStatusQueryService
{
   Task<Either<OrderNotFound, IEnumerable<OrderWithItemsDTO>>> GetOrdersByStatusAsync(string status);
}
