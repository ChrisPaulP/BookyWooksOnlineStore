namespace BookWooks.OrderApi.UseCases.Orders.List;
public record GetOrdersByStatusQuery(int? Skip, int? Take, string Status) : IQuery<OrdersByStatusResult>;
