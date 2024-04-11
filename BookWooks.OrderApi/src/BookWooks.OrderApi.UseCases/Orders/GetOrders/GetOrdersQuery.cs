namespace BookWooks.OrderApi.UseCases.Orders.GetOrders;
public record GetOrdersQuery(int? Skip, int? Take) : IQuery<Result<IEnumerable<OrderDTO>>>;

