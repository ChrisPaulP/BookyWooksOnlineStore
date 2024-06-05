

using Tracing;

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
    s.ExampleRequest = new CreateOrderRequest { Address = new()};
  });
}

public override async Task HandleAsync(
  CreateOrderRequest request,
  CancellationToken ct)
{
    using var activity = ActivitySourceProvider.Source.StartActivity();
    activity?.AddEvent(new("Order creation started."));
    OpenTelemetryMetricConfiguration.OrderStartedEventCounter.Add(1, new KeyValuePair<string, object?>("event.name", "OrderCreatedEvent"));
    var result = await _mediator.Send(new CreateOrderCommand(request.OrderItems.ToOrderCommandOrderItems(), request.CustomerId, new Address(request.Address.Street, request.Address.City, request.Address.Country, request.Address.Postcode), new PaymentDetails(request.Payment.CardNumber, request.Payment.CardHolderName, request.Payment.ExpiryDate, request.Payment.Cvv, request.Payment.PaymentMethod)));
    
    if (result.IsSuccess)
  {
      activity?.AddEvent(new("Order created successfully."));
      Response = new CreateOrderResponse(result.Value, request.CustomerId);
  }

}
}

