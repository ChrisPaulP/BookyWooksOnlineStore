namespace BookWooks.OrderApi.UseCases.Orders.OrderFulfillment;
public class OrderFulfillmentHandler : ICommandHandler<OrderFulfillmentCommand, Result<Guid>>
{
  private readonly IRepository<Order> _repository;

  public OrderFulfillmentHandler(IRepository<Order> repository)
  {
    _repository = repository;
  }

  public async Task<Result<Guid>> Handle(OrderFulfillmentCommand request,
    CancellationToken cancellationToken)
  {
    var spec = new OrderByIdSpec(request.OrderId);
    var entity = await _repository.FindAsync(spec);
    if (entity == null) return Result.NotFound("Project not found.");

    entity.FulfillOrder();
     _repository.Update(entity);

    return Result.Success();
  }
}
