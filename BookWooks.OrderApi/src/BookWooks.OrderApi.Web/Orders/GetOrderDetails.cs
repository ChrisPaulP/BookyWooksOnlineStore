using Microsoft.AspNetCore.Authorization;

namespace BookWooks.OrderApi.Web.Orders;

//[Authorize]
public class GetOrderDetails : Endpoint<GetOrderDetailsRequest, GetOrderDetailsResponse>
{
  private readonly IMediator _mediator;

  public GetOrderDetails(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get(GetOrderDetailsRequest.Route);
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetOrderDetailsRequest request,
  CancellationToken ct)
  {
    var query = new GetOrderDetailsQuery(request.OrderId);

    var result = await _mediator.Send(query);

    if (result.Status == ResultStatus.NotFound)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    if (result.IsSuccess)
    {
      Response = new GetOrderDetailsResponse(result.Value.Id,
        result.Value.Status,
        orderItems:
        result.Value.OrderItems?
          .Select(item => new OrderItemRecord(
            item.ProductId,
            item.Price,
            item.Quantity
            ))
          .ToList() ?? new List<OrderItemRecord>()
          );
    }
  }
}
