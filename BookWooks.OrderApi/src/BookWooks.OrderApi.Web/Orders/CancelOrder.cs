namespace BookWooks.OrderApi.Web.Orders;
public class CancelOrder : IEndpoint
{
  public void MapEndpoint(WebApplication app) => app
             .MapPost(CancelOrderRequest.Route, HandleAsync)
             .AddEndpointFilter<ValidationFilter<CancelOrderRequest>>();
  

  public static async Task<IResult> HandleAsync([FromBody] CancelOrderRequest request, IMediator mediator, CancellationToken ct) =>

    (await mediator.Send(new CancelOrderCommand(request.OrderId)))
    .Match((order) => Results.Ok(new CancelOrderResponse()),
           (error) => Results.NotFound(error)); 
}
