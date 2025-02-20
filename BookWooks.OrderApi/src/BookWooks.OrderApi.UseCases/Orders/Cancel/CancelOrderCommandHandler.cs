using BookyWooks.SharedKernel.Commands;
using BookyWooks.SharedKernel.Repositories;
using BookyWooks.SharedKernel.ResultPattern;

namespace BookWooks.OrderApi.UseCases.Contributors.Create;
public class CancelOrderHandler : ICommandHandler<CancelOrderCommand, StandardResult>
{
  private readonly IRepository<Order> _repository;

  private readonly ILogger<CreateOrderCommandHandler> _logger;

  public CancelOrderHandler(IRepository<Order> repository, ILogger<CreateOrderCommandHandler> logger)
  {
    _repository = repository;
    _logger = logger;
  }

  public async Task<StandardResult> Handle(CancelOrderCommand request,
    CancellationToken cancellationToken)
  {
    _logger.LogInformation("Setting order  {orderId} to cancelled", request.Id);
    var orderToCancel = await _repository.GetByIdAsync(request.Id);
    if (orderToCancel == null) return StandardResult.NotFound("could not find");

    orderToCancel.CancelOrder();
    _repository.Update(orderToCancel);
    await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    return StandardResult.Success();
  }
}

