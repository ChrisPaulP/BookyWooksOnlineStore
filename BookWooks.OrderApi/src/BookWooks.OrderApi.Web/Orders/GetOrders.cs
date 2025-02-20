using BookWooks.OrderApi.UseCases.Orders.GetOrders;

using LanguageExt;

using Microsoft.AspNetCore.Authorization;


namespace BookWooks.OrderApi.Web.Orders;

[Authorize]
public class GetOrders : IEndpoint
{
  public void MapEndpoint(WebApplication app)
  {
    app.MapGet("/Orders", HandleAsync);
  }

  private static async Task<IResponse> HandleAsync(
      IMediator mediator,
      CancellationToken ct)
  {
    var query = new GetOrdersQuery(0, 0);

    var result = await mediator.Send(query, ct);

    return result.Match<IResponse>(
            Right: orders => new GetOrdersResponse(orders.Select(order => new OrderRecord(Id: order.Id, Status: order.Status))),
            Left: error =>  new OrdersNotFoundResponse("No orders were found", StatusCodes.Status404NotFound, error));
  }
}
