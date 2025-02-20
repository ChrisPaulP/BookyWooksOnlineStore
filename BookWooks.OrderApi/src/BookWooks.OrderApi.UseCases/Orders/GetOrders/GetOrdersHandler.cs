namespace BookWooks.OrderApi.UseCases.Orders.GetOrders;

  public class GetOrdersHandler : IQueryHandler<GetOrdersQuery, Either<OrderNotFound, Seq<OrderDTO>>>
  {
    private readonly IReadRepository<Order> _repository;

    public GetOrdersHandler(IReadRepository<Order> repository)
    {
      _repository = repository;
    }

    public async Task<Either<OrderNotFound, Seq<OrderDTO>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
    return (await _repository.ListAllAsync())
               .Map(OrderMappingExtensions.ToOrderDTO)
               .ToEither(() => new OrderNotFound());
    }
}

