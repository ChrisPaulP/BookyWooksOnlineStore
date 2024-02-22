
using Ardalis.GuardClauses;



using BookWooks.OrderApi.Core.OrderAggregate;
using BookWooks.OrderApi.UseCases.Cancel;
using BookWooks.OrderApi.UseCases.Orders;
using BookyWooks.SharedKernel;
using Microsoft.Extensions.Logging;

namespace BookWooks.OrderApi.UseCases.Contributors.Create;
public class CancelOrderHandler : ICommandHandler<CancelOrderCommand, Result>
{
  private readonly IRepository<Order> _repository;

  private readonly ILogger<CreateOrderCommandHandler> _logger;

  public CancelOrderHandler(IRepository<Order> repository, ILogger<CreateOrderCommandHandler> logger)
  {
    _repository = repository;
    _logger = logger;
  }

  public async Task<Result> Handle(CancelOrderCommand request,
    CancellationToken cancellationToken)
  {
    _logger.LogInformation("Setting order  {orderId} to cancelled", request.Id);
    var orderToCancel = await _repository.GetByIdAsync(request.Id);
    if (orderToCancel == null) return Result.NotFound();

    orderToCancel.OrderCancelled();
    _repository.Update(orderToCancel);
    await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    return Result.Success();
  }
}

