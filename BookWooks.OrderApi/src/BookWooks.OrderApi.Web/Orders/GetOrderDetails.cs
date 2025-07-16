using BookyWooks.SharedKernel.Validation;

namespace BookWooks.OrderApi.Web.Orders;

public class GetOrderDetails : IEndpoint
{
    public void MapEndpoint(WebApplication app) => app
               .MapPost(GetOrderDetailsRequest.Route, HandleAsync)
               .AddEndpointFilter<ValidationFilter<CancelOrderRequest>>()
               .AllowAnonymous();
    
    public static async Task<IResult> HandleAsync([FromBody] GetOrderDetailsRequest request, IMediator mediator, CancellationToken ct) =>(
             await mediator.Send(new GetOrderDetailsQuery(request.OrderId)))
            .Match(
             order => Results.Ok(new GetOrderDetailsResponse(order.Id,order.Status,new List<OrderItemRecord>())),
             orderError => ToHttpResult.Map(orderError));
}
