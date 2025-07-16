namespace BookWooks.OrderApi.Web.Orders;

public class GetOrderByStatus : IEndpoint
{
    public void MapEndpoint(WebApplication app) => app
          .MapGet(GetOrderByStatusRequest.Route, HandleAsync)
          .AllowAnonymous()
          .AddEndpointFilter<ValidationFilter<CreateOrderRequest>>();
    

    private static async Task<IResult> HandleAsync([FromBody] GetOrderByStatusRequest request, IMediator mediator, CancellationToken ct) =>(

        await mediator.Send(new GetOrdersByStatusQuery(null, null, request.Status), ct))
        .Match(
         orders => Results.Ok(new GetOrderByStatusResponse(orders.Select(o => new OrderWithItemsRecord(o.Id,o.Status,o.OrderItems?.ToOrderItemRecord() ?? [])).ToList())),
         orderNotFound => ToHttpResult.Map(orderNotFound));   
}
