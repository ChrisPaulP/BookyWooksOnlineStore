using BookyWooks.SharedKernel.Validation;

namespace BookWooks.OrderApi.Web.Orders;
public record GetOrdersResponse(IEnumerable<OrderRecord> orders) : IResponse;
public record OrdersNotFoundResponse(string Message, int StatusCode, Error Error) : IResponse;
