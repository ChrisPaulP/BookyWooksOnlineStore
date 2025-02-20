namespace BookWooks.OrderApi.UseCases.Orders.Get;
public class GetOrderDetailsHandler : IQueryHandler<GetOrderDetailsQuery, Option<OrderDTO>>
{
  private readonly IReadRepository<Order> _readRepository;

  public GetOrderDetailsHandler(IReadRepository<Order> readRepository) => _readRepository = readRepository;

    public async Task<Option<OrderDTO>> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken) =>
        (await _readRepository.FindAsync(new OrderByIdSpec(request.OrderId)))
            .ToOption()
            .Map(order => new OrderDTO(
                order.Id,
                order.Status.Label
            ));
}

