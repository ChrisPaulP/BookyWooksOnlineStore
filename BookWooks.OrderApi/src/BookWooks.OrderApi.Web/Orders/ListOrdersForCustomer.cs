

using BookWooks.OrderApi.UseCases.Orders.ListOrdersForCustomer;

namespace BookWooks.OrderApi.Web.Orders;

public class ListOrdersForCustomer : Endpoint<ListOrdersForCustomerRequest, ListOrdersForCustomerResponse>
{
  private readonly IMediator _mediator;

  public ListOrdersForCustomer(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get(ListOrdersForCustomerRequest.Route);
    AllowAnonymous();
  }

  public override async Task HandleAsync(ListOrdersForCustomerRequest request,
    CancellationToken ct)
  {
    var command = new ListOrdersForCustomerQuery(null, null, request.CustomerId);

    var result = await _mediator.Send(command);

    if (result.Status == ResultStatus.NotFound)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    if (result.IsSuccess)
    {
      Response = new ListOrdersForCustomerResponse
      {
        Orders = result.Value.Select(o => new OrderRecord(o.Id, o.Status, o.OrderItems?.ToOrderItemRecord() ?? new List<OrderItemRecord>())).ToList()
      };
    }
  }
}
