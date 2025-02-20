namespace BookWooks.OrderApi.UseCases.Orders.ListOrdersForCustomer;
public class ListOrdersForCustomerHandler : IQueryHandler<ListOrdersForCustomerQuery, Either<OrderNotFound, Seq<OrderDTO>>> 
{
  private readonly IReadRepository<Order> _readRepository;
  private readonly ILogger<ListOrdersForCustomerHandler> _logger;

  public ListOrdersForCustomerHandler(IReadRepository<Order> readRepository, ILogger<ListOrdersForCustomerHandler> logger)
  {
    _readRepository = readRepository;
    _logger = logger;
  }

  public async Task<Either<OrderNotFound, Seq<OrderDTO>>> Handle(ListOrdersForCustomerQuery request, CancellationToken cancellationToken)
  {
    var spec = new OrderByCustomerIdSpec(request.CustomerId);
    //var order = await _repository.FindAsync(spec);
    var order = await _readRepository.FindAllAsync(spec);
    return order.Map(OrderMappingExtensions.ToOrderDTO)
               .ToEither(() => new OrderNotFound());
  }
}
