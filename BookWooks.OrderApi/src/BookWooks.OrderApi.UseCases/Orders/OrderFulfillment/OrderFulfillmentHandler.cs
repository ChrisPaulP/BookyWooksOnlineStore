using OrderErrors = BookWooks.OrderApi.UseCases.Errors.OrderErrors;

namespace BookWooks.OrderApi.UseCases.Orders.OrderFulfillment;
public class OrderFulfillmentHandler : ICommandHandler<OrderFulfillmentCommand, SetOrderStatusResult>
{
  private readonly IRepository<Order> _repository;
  private readonly ILogger<OrderFulfillmentHandler> _logger;

  public OrderFulfillmentHandler(IRepository<Order> repository, ILogger<OrderFulfillmentHandler> logger) => (_repository, _logger) = (repository, logger);

  public async Task<SetOrderStatusResult> Handle(OrderFulfillmentCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Setting order {orderId} to cancelled", request.Id);

    var findOrder = await _repository.FindAsync(new OrderByIdSpec(OrderId.From(request.OrderId)));
    return await findOrder
        .ToEither<OrderErrors, Order>(() => new OrderNotFound())
        .Map(order => order.FulfillOrder())
        .SaveOrder(_repository.UpdateAsync, _repository, cancellationToken)
        .MapAsync(OrderMappingExtensions.ToOrderDTO);
  }
}

