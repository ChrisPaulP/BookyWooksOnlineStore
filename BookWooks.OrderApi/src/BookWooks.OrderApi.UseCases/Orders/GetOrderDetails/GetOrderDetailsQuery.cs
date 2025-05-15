namespace BookWooks.OrderApi.UseCases.Orders.Get;
public record GetOrderDetailsQuery(Guid OrderId) : IQuery<OrderDetailsResult>;

