

namespace BookWooks.OrderApi.Web.Orders;

public class Create : IEndpoint
{
  public void MapEndpoint(WebApplication app) => app
             .MapPost(CreateOrderRequest.Route, HandleAsync)
             .AddEndpointFilter<ValidationFilter<CreateOrderRequest>>();
  

  private static async Task<IResult> HandleAsync([FromBody] CreateOrderRequest request, IMediator mediator, IDiagnosticsActivityLogger diagnosticsActivityLogger, CancellationToken ct)
  {
    diagnosticsActivityLogger.LogStart(ApplicationName.Order, Event.OrderStarted, "Processing order creation");

    return (await mediator.Send(CreateOrderCommandBuilder.Build(request), ct))
      .Match(
            orderId =>
            {
              diagnosticsActivityLogger.LogSuccess("Order successfully created");
              return Results.Ok(new CreateOrderResponse(orderId.Value, request.CustomerId));
            },
            validationErrors =>
            {
              var validationError = validationErrors.Errors.FirstOrDefault();
              diagnosticsActivityLogger.LogError($"Order creation failed: {validationError}");
              return Results.BadRequest(new BusinessRuleViolationsResponse("Business rule errors", StatusCodes.Status400BadRequest, validationErrors));
            }
        );
  }
}

