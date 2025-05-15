namespace BookWooks.OrderApi.UseCases.Orders.GetOrders;

public class GetOrdersHandler : IQueryHandler<GetOrdersQuery, OrdersResult>
{
    private readonly IReadRepository<Order> _repository;

    public GetOrdersHandler(IReadRepository<Order> repository) => _repository = repository;

    public async Task<OrdersResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken) =>

      (await _repository
            .ListAllAsync())
            .ToEither(() => new OrderNotFound())
            .Map(orders => orders.Select(OrderMappingExtensions.ToOrderDTO));
}
