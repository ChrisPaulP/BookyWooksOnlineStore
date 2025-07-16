using OrderErrors = BookWooks.OrderApi.UseCases.Errors.OrderErrors;

namespace BookWooks.OrderApi.UseCases.Orders.List;
public interface IGetOrdersByStatusQueryService
{
   Task<Either<OrderErrors, IEnumerable<OrderWithItemsDTO>>> GetOrdersByStatusAsync(string status);
}
