using BookWooks.OrderApi.UseCases.Errors;
using BookyWooks.SharedKernel.ResultPattern;
using BookyWooks.SharedKernel.Validation;

namespace BookWooks.OrderApi.Web.Orders;

public class GetOrderByStatus : IEndpoint
{
  public void MapEndpoint(WebApplication app)
  {
    app.MapGet(GetOrderByStatusRequest.Route, HandleAsync)
      .AllowAnonymous()
      .AddEndpointFilter<ValidationFilter<CreateOrderRequest>>();
  }

  private static async Task<IResponse> HandleAsync(
    GetOrderByStatusRequest request,
    IMediator mediator,
    CancellationToken ct)
  {
    var query = await mediator.Send(new GetOrdersByStatusQuery(null, null, request.Status), ct);

    return query.Any()
        ? new GetOrderByStatusResponse(
            query.Select(o => new OrderWithItemsRecord(
                o.Id,
                o.Status,
                o.OrderItems?.ToOrderItemRecord() ?? []
            )).ToList()
        )
        : new OrderNotFoundResponse("Order was not found", StatusCodes.Status404NotFound, new OrderNotFound());
  }
}
