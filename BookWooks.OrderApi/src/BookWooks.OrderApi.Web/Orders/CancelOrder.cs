namespace BookWooks.OrderApi.Web.Orders;

public class CancelOrder : Endpoint<CancelOrderRequest, CancelOrderResponse>
{
  private readonly IMediator _mediator;

  public CancelOrder(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Post(CancelOrderRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.ExampleRequest = new CancelOrderRequest { OrderId = Guid.NewGuid() };
    });
  }

  public override async Task HandleAsync(
    CancelOrderRequest request,
    CancellationToken ct)
  {
    var result = await _mediator.Send(new CancelOrderCommand(request.OrderId));

    if (result.IsSuccess)
    {
      Response = new CancelOrderResponse();
    }
  }
}
