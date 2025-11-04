namespace BookWooks.OrderApi.Web.Orders;

public record CreateOrderResponse(Guid Id, Guid CustomerId);
public record ValidationProblemDetails(int StatusCode, string title, string detail, string instance, IDictionary<string, string[]> errors);


