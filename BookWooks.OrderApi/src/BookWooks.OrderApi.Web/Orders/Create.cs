namespace BookWooks.OrderApi.Web.Orders;
public class Create : Endpoint<CreateOrderRequest, CreateOrderResponse>
{
  private readonly IMediator _mediator;
 
  public Create(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
{
  Post(CreateOrderRequest.Route);
  AllowAnonymous();
  Summary(s =>
  {
    s.ExampleRequest = new CreateOrderRequest { Name = "Order Customer Name" };
  });
}

public override async Task HandleAsync(
  CreateOrderRequest request,
  CancellationToken ct)
{
  var result = await _mediator.Send(new CreateOrderCommand(request.Id, request.OrderItems.ToOrderCommandOrderItems(), request.UserId!, request.UserName!, request.City!, request.Street!, request.Country!, request.Postcode!, request.CardNumber!, request.CardHolderName!, request.ExpiryDate!, request.CardSecurityNumber!));

  if (result.IsSuccess)
  {
    Response = new CreateOrderResponse(result.Value, request.UserName!);
  }

}
}

