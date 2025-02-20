namespace BookWooks.OrderApi.UseCases.Orders.Create;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Either<ValidationErrors, Guid>>
{
  private readonly ILogger<CreateOrderCommandHandler> _logger;
  private readonly IRepository<Order> _orderRepository;

  public CreateOrderCommandHandler(ILogger<CreateOrderCommandHandler> logger, IRepository<Order> orderRepository)
  {
    _logger = logger;
    _orderRepository = orderRepository;
  }

  public async Task<Either<ValidationErrors, Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling CreateOrderCommand for CustomerId: {CustomerId}", request.CustomerId);

    var orderResult = await request
        .CreateOrder()
        .AddOrderItems(request.OrderItems)
        .SaveOrder(_orderRepository, cancellationToken, _logger);

    return orderResult.Match<Either<ValidationErrors, Guid>>(
        order => order.Id,
        errors => new ValidationErrors(errors.Errors));
  }
}


