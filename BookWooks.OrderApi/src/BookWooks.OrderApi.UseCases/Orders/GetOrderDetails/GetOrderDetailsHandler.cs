namespace BookWooks.OrderApi.UseCases.Orders.Get;
public class GetOrderDetailsHandler : IQueryHandler<GetOrderDetailsQuery, OrderDetailsResult>
{
  private readonly IReadRepository<Order> _readRepository;

  public GetOrderDetailsHandler(IReadRepository<Order> readRepository) => _readRepository = readRepository;

    public async Task<OrderDetailsResult> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken) =>
        (await _readRepository
               .FindAsync(new OrderByIdSpec(request.OrderId)))
               .ToEither(() => new OrderNotFound())
               .Map(OrderMappingExtensions.ToOrderDTO);
}
