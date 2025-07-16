namespace BookWooks.OrderApi.Web.Orders;

[Authorize]
public class GetOrders : IEndpoint
{
    public void MapEndpoint(WebApplication app) => app.MapGet("/Orders", HandleAsync);
   
    private static async Task<IResult> HandleAsync(IMediator mediator) =>(
                await mediator.Send(new GetOrdersQuery(0, 0))).Match(
                orders => Results.Ok(new GetOrdersResponse(orders.Select(order => new OrderRecord(Id: order.Id, Status: order.Status)))),
                ordersNotFound => ToHttpResult.Map(ordersNotFound));    
}
