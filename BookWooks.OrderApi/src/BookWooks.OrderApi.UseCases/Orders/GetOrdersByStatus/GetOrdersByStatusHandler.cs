using BookyWooks.SharedKernel;

namespace BookWooks.OrderApi.UseCases.Orders.List;
public class GetOrdersByStatusHandler : IQueryHandler<GetOrdersByStatusQuery, DetailedResult<IEnumerable<OrderDTO>>>
{
  private readonly IGetOrdersByStatusQueryService _query;
  private readonly IRepository<Order> _repository;
  private readonly ILogger<GetOrdersByStatusHandler> _logger;

  public GetOrdersByStatusHandler(IGetOrdersByStatusQueryService query, IRepository<Order> repository, ILogger<GetOrdersByStatusHandler> logger)
  {
    _query = query;
    _repository = repository;
    _logger = logger;
  }

  public async Task<DetailedResult<IEnumerable<OrderDTO>>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
  {
    //var result = await _query.ListOrdersAsync();
    var result = await _query.GetOrdersByStatusAsync(request.Status); // test to see what this does

    //var spec = new OrderByStatusSpec(request.orderStatus);
    //var ordersBySpec = await _repository.FindAllAsync(spec);

    //if (ordersBySpec == null)
    //{
    //  return Result.NotFound();
    //}

    //var ordersBySpecDtoList = ordersBySpec
    //    .Select(order => new OrderDTO(
    //        order.Id,
    //        order.Status.Name,
    //        order.OrderItems.Select(item => new OrderItemDTO(item.BookId, item.BookTitle, item.BookPrice, item.Quantity))//.ToList()
    //    ))
    //    .ToList();

    //return StandardResult.Success(result); can also call this
    return DetailedResult<IEnumerable<OrderDTO>>.Success(result);
  }
}
