

namespace BookWooks.OrderApi.Web.Orders;

public class Create : IEndpoint
{
  public void MapEndpoint(WebApplication app) => app
             .MapPost(CreateOrderRequest.Route, HandleAsync)
             .AddEndpointFilter<ValidationFilter<CreateOrderRequest>>();


  public static async Task<IResult> HandleAsync([FromBody] CreateOrderRequest request, IMediator mediator, IDiagnosticsActivityLogger diagnosticsActivityLogger, CancellationToken ct)
  {
    diagnosticsActivityLogger.LogStart(ApplicationName.Order, Event.OrderStarted, "Processing order creation");

    return (await mediator.Send(CreateOrderCommandBuilder.Build(request), ct))
        .Match(
            orderId => Results.Ok(new CreateOrderResponse(orderId.Value, request.CustomerId)),
            createOrderErrors =>
            {
              diagnosticsActivityLogger.LogError($"Order creation failed: {createOrderErrors}");
              return ToHttpResult.Map(createOrderErrors);
            }
        );
  }
}

