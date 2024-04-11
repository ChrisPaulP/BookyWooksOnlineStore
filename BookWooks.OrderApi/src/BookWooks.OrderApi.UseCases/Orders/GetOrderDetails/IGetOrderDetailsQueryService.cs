namespace BookWooks.OrderApi.UseCases.Orders.Get;
public interface IGetOrderDetailsQueryService
{
  Task<IEnumerable<OrderDTO>> ListOrdersAsync();
}

