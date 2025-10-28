
using OrderErrors = BookWooks.OrderApi.UseCases.Errors.OrderErrors;

namespace BookWooks.OrderApi.UseCases.Orders.OrderFulfillment;

public class OrderFulfillmentHandler : ICommandHandler<OrderFulfillmentCommand, SetOrderStatusResult>
{
  private readonly IRepository<Order> _repository;
  private readonly ILogger<OrderFulfillmentHandler> _logger;

  public OrderFulfillmentHandler(IRepository<Order> repository, ILogger<OrderFulfillmentHandler> logger)
      => (_repository, _logger) = (repository, logger);

  public async Task<SetOrderStatusResult> Handle(OrderFulfillmentCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Setting order {OrderId} to fulfilled", request.OrderId);

    return await (await _repository.FindAsync(new OrderByIdSpec(OrderId.From(request.OrderId))))
          .ToEither<OrderErrors, Order>(() => new OrderNotFound())
          .Bind(order => order.FulfillOrder().ToEither(domainError => new FulfillOrderError(domainError.Error.Message)))
          .SaveOrder(_repository.UpdateAsync, _repository, cancellationToken)
          .MapAsync(OrderMappingExtensions.ToOrderDTO);
  }
}
  


