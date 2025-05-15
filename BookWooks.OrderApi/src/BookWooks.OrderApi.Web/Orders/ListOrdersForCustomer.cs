namespace BookWooks.OrderApi.Web.Orders;
public class ListOrdersForCustomer : IEndpoint
{
    public void MapEndpoint(WebApplication app) => app
               .MapPost(ListOrdersForCustomerRequest.Route, HandleAsync)
               .AddEndpointFilter<ValidationFilter<CancelOrderRequest>>().AllowAnonymous();
    
    public static async Task<IResult> HandleAsync(ListOrdersForCustomerRequest request, IMediator mediator, CancellationToken ct) =>
    
        (await mediator.Send(new ListOrdersForCustomerQuery(null, null, request.CustomerId)))
                       .Match(
                              orders => Results.Ok(new ListOrdersForCustomerResponse{Orders = orders.Select(order => new OrderRecord(order.Id, order.Status)).ToList()}),
                              (error) => Results.NotFound(error));    
}
