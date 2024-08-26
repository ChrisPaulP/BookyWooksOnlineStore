namespace BookWooks.OrderApi.UseCases.Orders.OrderFulfillment;
public class OrderFulfillmentHandler : ICommandHandler<OrderFulfillmentCommand, DetailedResult<Guid>>
{
  private readonly IRepository<Order> _repository;

  public OrderFulfillmentHandler(IRepository<Order> repository)
  {
    _repository = repository;
  }

  public async Task<DetailedResult<Guid>> Handle(OrderFulfillmentCommand request,
    CancellationToken cancellationToken)
  {
    var spec = new OrderByIdSpec(request.OrderId);
    var entity = await _repository.FindAsync(spec);
    if (entity == null) return StandardResult.NotFound("Project not found.");

    entity.FulfillOrder();
     _repository.Update(entity);

    return StandardResult.Success();
  }
}
