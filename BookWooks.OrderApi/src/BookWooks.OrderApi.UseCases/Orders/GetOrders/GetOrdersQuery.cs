using BookyWooks.SharedKernel.Queries;
using BookyWooks.SharedKernel.ResultPattern;
using BookyWooks.SharedKernel.Validation;


namespace BookWooks.OrderApi.UseCases.Orders.GetOrders;
//public record GetOrdersQuery(int? Skip, int? Take) : IQuery<DetailedResult<IEnumerable<OrderDTO>>>;

public record GetOrdersQuery(int? Skip, int? Take) : IQuery<Either<OrderNotFound, Seq<OrderDTO>>>;

