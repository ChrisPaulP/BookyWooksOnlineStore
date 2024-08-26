namespace BookWooks.OrderApi.UseCases.Orders.ListOrdersForCustomer;
public class ListOrdersForCustomerHandler : IQueryHandler<ListOrdersForCustomerQuery, DetailedResult<IEnumerable<OrderDTO>>>
{
  private readonly IReadRepository<Order> _readRepository;
  private readonly ILogger<ListOrdersForCustomerHandler> _logger;

  public ListOrdersForCustomerHandler(IReadRepository<Order> readRepository, ILogger<ListOrdersForCustomerHandler> logger)
  {
    _readRepository = readRepository;
    _logger = logger;
  }

  public async Task<DetailedResult<IEnumerable<OrderDTO>>> Handle(ListOrdersForCustomerQuery request, CancellationToken cancellationToken)
  {
    var spec = new OrderByCustomerIdSpec(request.CustomerId);
    //var order = await _repository.FindAsync(spec);
    var order = await _readRepository.FindAllAsync(spec);
    if (order == null)
    {
      return StandardResult.NotFound();
    }
    var ordersByCustomerDtoList = order
        .Select(order => new OrderDTO(
            order.Id,
            order.Status.Label,
            order.OrderItems.Select(item => new OrderItemDTO(item.ProductId, item.Price, item.Quantity))//.ToList()
        ))
        .ToList();

    return ordersByCustomerDtoList;
  }
}
