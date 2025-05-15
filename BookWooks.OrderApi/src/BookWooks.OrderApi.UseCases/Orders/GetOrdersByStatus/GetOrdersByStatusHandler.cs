namespace BookWooks.OrderApi.UseCases.Orders.List;
public class GetOrdersByStatusHandler : IQueryHandler<GetOrdersByStatusQuery, OrdersByStatusResult>
{
  private readonly IGetOrdersByStatusQueryService _query;

  public GetOrdersByStatusHandler(IGetOrdersByStatusQueryService query, ILogger<GetOrdersByStatusHandler> logger) => (_query) = (query);

  public async Task<OrdersByStatusResult> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken) => await _query.GetOrdersByStatusAsync(request.Status);
}
