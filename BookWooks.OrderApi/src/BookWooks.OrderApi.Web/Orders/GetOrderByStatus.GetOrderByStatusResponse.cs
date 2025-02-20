

using BookyWooks.SharedKernel.Validation;

namespace BookWooks.OrderApi.Web.Orders;

public record GetOrderByStatusResponse(List<OrderWithItemsRecord> Orders) : IResponse;
public record OrderNotFoundResponse(string Message, int StatusCode, Error Error) : IResponse;
