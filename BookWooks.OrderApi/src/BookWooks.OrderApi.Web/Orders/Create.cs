
namespace BookWooks.OrderApi.Web.Orders;

public class Create : IEndpoint
{
  public void MapEndpoint(WebApplication app)
  {
    app.MapPost(CreateOrderRequest.Route, HandleAsync)
      .AddEndpointFilter<ValidationFilter<CreateOrderRequest>>();
  }

  private static async Task<IResponse> HandleAsync(
      CreateOrderRequest request,
      IMediator mediator,
      IDiagnosticsActivityLogger diagnosticsActivityLogger,
      CancellationToken ct)
  {
    diagnosticsActivityLogger.LogStart(ApplicationName.Order, Event.OrderStarted, "Processing order creation");

    var result = await mediator.Send(new CreateOrderCommand(
      request.CustomerId, 
      new Address(request.Address.Street, request.Address.City, request.Address.Country, request.Address.Postcode), 
      new PaymentDetails(request.Payment.CardHolderNumber, request.Payment.CardHolderName, request.Payment.ExpiryDate, request.Payment.PaymentMethod),
      request.OrderItems.Select(x => new OrderItem(x.ProductId, x.ProductName, x.ProductDescription, x.Price, x.Quantity))), 
      ct);

    return result.Match<IResponse>(
            Right: orderId =>
            {
              diagnosticsActivityLogger.LogSuccess("Order successfully created");
              return new CreateOrderResponse(orderId, request.CustomerId);
            },
            Left: errors =>
            {
              var validationError = errors.Errors.FirstOrDefault();
              diagnosticsActivityLogger.LogError($"Order creation failed: {validationError}");
              return new BusinessRuleViolationsResponse("Business rule errors", StatusCodes.Status400BadRequest, errors);
            }
        );
  }
}

