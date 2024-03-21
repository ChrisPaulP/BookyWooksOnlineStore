namespace BookWooks.OrderApi.Web.Orders;

public class GetOrderByStatus : Endpoint<GetOrderByStatusRequest, GetOrderByStatusResponse>
{
  private readonly IMediator _mediator;

  public GetOrderByStatus(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get(GetOrderByStatusRequest.Route);
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetOrderByStatusRequest request,
    CancellationToken ct)
  {
    var command = new GetOrdersByStatusQuery(null, null, request.Status);

    var result = await _mediator.Send(command);

    if (result.Status == ResultStatus.NotFound)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    if (result.IsSuccess)
    {
      Response = new GetOrderByStatusResponse
      {
        Orders = result.Value.Select(o => new OrderRecord(o.Id, o.Status, o.OrderItems?.ToOrderItemRecord() ?? new List<OrderItemRecord>())).ToList()
      };
    }
  }
}
