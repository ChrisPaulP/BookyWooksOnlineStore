using OrderErrors = BookWooks.OrderApi.UseCases.Errors.OrderErrors;

namespace BookWooks.OrderApi.UseCases.Contributors.Cancel;

public class CancelOrderHandler : ICommandHandler<CancelOrderCommand, CancelOrderResult>
{
    private readonly IRepository<Order> _repository;
    private readonly ILogger<CancelOrderHandler> _logger;

    public CancelOrderHandler(IRepository<Order> repository, ILogger<CancelOrderHandler> logger) 
        => (_repository, _logger) = (repository, logger);

    public async Task<CancelOrderResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting order {orderId} to cancelled", request.Id);
        
        return await (await _repository.GetByIdAsync(request.Id))
            .ToEither<OrderErrors, Order>(() => new OrderNotFound())
            .Bind(order => order.CancelOrder()
            .ToEither(domainError => new CancelOrderError(domainError.Error.Message)))
            .SaveOrder(_repository.UpdateAsync, _repository, cancellationToken)
            .MapAsync(OrderMappingExtensions.ToOrderCancelledDTO);
  }
}
