using OrderErrors = BookWooks.OrderApi.UseCases.Errors.OrderErrors;
namespace BookWooks.OrderApi.UseCases.Orders.ListOrdersForCustomer;
public class ListOrdersForCustomerHandler : IQueryHandler<ListOrdersForCustomerQuery, OrdersResult>
{
  private readonly IReadRepository<Order> _readRepository;
  private readonly ILogger<ListOrdersForCustomerHandler> _logger;

  public ListOrdersForCustomerHandler(IReadRepository<Order> readRepository, ILogger<ListOrdersForCustomerHandler> logger) => (_readRepository, _logger) = (readRepository, logger);

    public async Task<OrdersResult> Handle(ListOrdersForCustomerQuery request, CancellationToken cancellationToken) =>
         
    (await _readRepository.FindAllAsync(new OrderByCustomerIdSpec(CustomerId.From(request.CustomerId))))
                          .ToEither<OrderErrors, Order>(() => new OrderNotFound())
                          .Map(orders => orders.Map(OrderMappingExtensions.ToOrderDTO));
}
