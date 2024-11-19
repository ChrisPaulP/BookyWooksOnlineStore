
using Azure;
using BookWooks.OrderApi.UseCases.Orders.GetOrders;
using Microsoft.AspNetCore.Authorization;

namespace BookWooks.OrderApi.Web.Orders;
[Authorize]
public class GetOrders : EndpointWithoutRequest<GetOrdersResponse>
{
  private readonly IMediator _mediator;

  public GetOrders(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get("/Orders");
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var command = new GetOrdersQuery(0, 0);

    var result = await _mediator.Send(command);
    
    var orders = result.Value.Select(o => new OrderRecord(o.Id, o.Status, o.OrderItems?.ToOrderItemRecord() ?? new List<OrderItemRecord>())).ToList();
    if (result.Status == ResultStatus.NotFound)
    {
      await SendNotFoundAsync(ct);
      Response = new GetOrdersResponse(orders, result.Errors);
      return;
    } 
    Response = new GetOrdersResponse(orders);     
  }
}


