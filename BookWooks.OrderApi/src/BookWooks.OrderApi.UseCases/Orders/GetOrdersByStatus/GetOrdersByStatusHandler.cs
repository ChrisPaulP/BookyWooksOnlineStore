namespace BookWooks.OrderApi.UseCases.Orders.List;
public class GetOrdersByStatusHandler : IQueryHandler<GetOrdersByStatusQuery, IEnumerable<OrderWithItemsDTO>>
{
  private readonly IGetOrdersByStatusQueryService _query;
  private readonly ILogger<GetOrdersByStatusHandler> _logger;

  public GetOrdersByStatusHandler(IGetOrdersByStatusQueryService query, ILogger<GetOrdersByStatusHandler> logger)
  {
    _query = query;
    _logger = logger;
  }

  public async Task<IEnumerable<OrderWithItemsDTO>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
  {
    return (await _query.GetOrdersByStatusAsync(request.Status));
  }
}
