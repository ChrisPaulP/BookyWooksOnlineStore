using BookyWooks.SharedKernel.Validation;

namespace BookWooks.OrderApi.Web.Orders;
public record GetOrdersResponse(IEnumerable<OrderRecord> orders);
public record OrdersNotFoundResponse(string Message, Error Error);
