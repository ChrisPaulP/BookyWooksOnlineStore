using BookWooks.OrderApi.UseCases.Orders.GetOrders;
namespace BookWooks.OrderApi.Web.Orders;

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
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var command = new GetOrdersQuery(0, 0);

    var result = await _mediator.Send(command);

    if (result.Status == ResultStatus.NotFound)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    if (result.IsSuccess)
    {
      Response = new GetOrdersResponse
      {
        Orders = result.Value.Select(o => new OrderRecord(o.Id, o.Status, o.OrderItems?.ToOrderItemRecord() ?? new List<OrderItemRecord>())).ToList()
      };
    }
  }
}
