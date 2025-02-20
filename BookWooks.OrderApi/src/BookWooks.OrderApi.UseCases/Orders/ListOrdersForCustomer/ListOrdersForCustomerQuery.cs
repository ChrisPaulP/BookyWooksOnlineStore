using BookyWooks.SharedKernel.Queries;
using BookyWooks.SharedKernel.ResultPattern;

namespace BookWooks.OrderApi.UseCases.Orders.ListOrdersForCustomer;
public record ListOrdersForCustomerQuery(int? Skip, int? Take, Guid CustomerId) : IQuery<Either<OrderNotFound, Seq<OrderDTO>>>;
