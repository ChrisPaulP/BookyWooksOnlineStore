using OrderErrors = BookWooks.OrderApi.UseCases.Errors.OrderErrors;

namespace BookWooks.OrderApi.UseCases.Orders.Get;
public class GetOrderDetailsHandler : IQueryHandler<GetOrderDetailsQuery, OrderDetailsResult>
{
    private readonly IReadRepository<Order> _readRepository;

    public GetOrderDetailsHandler(IReadRepository<Order> readRepository) => _readRepository = readRepository;

    public async Task<OrderDetailsResult> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken) =>
        (await _readRepository
               .FindAsync(new OrderByIdSpec(OrderId.From(request.OrderId))))
               .ToEither<OrderErrors, Order>(() => new OrderNotFound())
               .Map(OrderMappingExtensions.ToOrderDTO);
}
