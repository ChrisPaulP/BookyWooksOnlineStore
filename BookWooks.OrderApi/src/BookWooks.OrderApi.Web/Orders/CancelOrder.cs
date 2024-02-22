namespace BookWooks.OrderApi.Web.Orders;

public class CancelOrder : Endpoint<CancelOrderRequest, CancelOrderResponse>
{
  private readonly IMediator _mediator;
  private readonly IRepository<Order> _repository;

  public CancelOrder(IRepository<Order> repository,
    IMediator mediator)
  {
    _mediator = mediator;
    _repository = repository;
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
    CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new CancelOrderCommand(request.OrderId));

    if (result.IsSuccess)
    {
      Response = new CancelOrderResponse();
      return;
    }
  }
}
