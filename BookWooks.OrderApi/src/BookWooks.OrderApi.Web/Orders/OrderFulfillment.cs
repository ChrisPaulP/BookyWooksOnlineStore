//using BookWooks.OrderApi.UseCases.Orders.OrderFulfillment;
//using BookyWooks.SharedKernel;
//using FastEndpoints;
//using MediatR;

//namespace BookWooks.OrderApi.Web.Orders;

//public class OrderFulfillment : Endpoint<OrderFulfillmentRequest>
//{
//  private readonly IMediator _mediator;

//  public OrderFulfillment(IMediator mediator)
//  {
//    _mediator = mediator;
//  }

//  public override void Configure()
//  {
//    Post(OrderFulfillmentRequest.Route);
//    AllowAnonymous();
//    Summary(s =>
//    {
//      s.ExampleRequest = new OrderFulfillmentRequest
//      {
//        OrderId = Guid.NewGuid()
//      };
//    });
//  }

//  public override async Task HandleAsync(
//    OrderFulfillmentRequest request,
//    CancellationToken cancellationToken)
//  {
//    var command = new OrderFulfillmentCommand(request.OrderId);
//    var result = await _mediator.Send(command);

//    if (result.Status == ResultStatus.NotFound)
//    {
//      await SendNotFoundAsync(cancellationToken);
//      return;
//    }

//    if (result.IsSuccess)
//    {
//      await SendNoContentAsync(cancellationToken);
//    };
//    // TODO: Handle other issues as needed
//  }

//}
