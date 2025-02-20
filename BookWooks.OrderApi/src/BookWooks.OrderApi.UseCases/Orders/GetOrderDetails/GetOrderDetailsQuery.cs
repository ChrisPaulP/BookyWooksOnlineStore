using BookyWooks.SharedKernel.Queries;
using BookyWooks.SharedKernel.ResultPattern;

namespace BookWooks.OrderApi.UseCases.Orders.Get;
public record GetOrderDetailsQuery(Guid OrderId) : IQuery<Option<OrderDTO>>;

