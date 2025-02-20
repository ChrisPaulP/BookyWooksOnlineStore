using BookyWooks.SharedKernel.Queries;
using BookyWooks.SharedKernel.ResultPattern;
using BookyWooks.SharedKernel.Validation;

namespace BookWooks.OrderApi.UseCases.Orders.List;
public record GetOrdersByStatusQuery(int? Skip, int? Take, string Status) : IQuery<IEnumerable<OrderWithItemsDTO>>;
