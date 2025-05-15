namespace BookWooks.OrderApi.Web.Orders;
public class OrderFulfillment : IEndpoint
{
  public void MapEndpoint(WebApplication app) => app
             .MapPost(OrderFulfillmentRequest.Route, HandleAsync)
             .AddEndpointFilter<ValidationFilter<OrderFulfillmentRequest>>();


  public static async Task<IResult> HandleAsync(OrderFulfillmentRequest request, IMediator mediator, CancellationToken ct) =>

    (await mediator.Send(new OrderFulfillmentCommand(request.OrderId)))
    .Match((order) => Results.Ok(new OrderFulfilledResponse()),
           (error) => Results.NotFound(error));
}
