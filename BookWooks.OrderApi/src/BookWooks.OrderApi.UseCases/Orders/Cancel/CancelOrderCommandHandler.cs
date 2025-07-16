using OrderErrors = BookWooks.OrderApi.UseCases.Errors.OrderErrors;

namespace BookWooks.OrderApi.UseCases.Contributors.Create;
public class CancelOrderHandler : ICommandHandler<CancelOrderCommand, CancelOrderResult>
{
  private readonly IRepository<Order> _repository;
  private readonly ILogger<CancelOrderHandler> _logger;

  public CancelOrderHandler(IRepository<Order> repository, ILogger<CancelOrderHandler> logger) => (_repository, _logger) = (repository, logger);

  public async Task<CancelOrderResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
      _logger.LogInformation("Setting order {orderId} to cancelled", request.Id);
      var findOrder = await _repository.GetByIdAsync(request.Id);
    
      return await findOrder
          .ToEither<OrderErrors, Order>(() => new OrderNotFound())
          .Map(order => order.CancelOrder())
          .SaveOrder(_repository.UpdateAsync, _repository, cancellationToken)
          .MapAsync(OrderMappingExtensions.ToOrderCancelledDTO);
    }
}
