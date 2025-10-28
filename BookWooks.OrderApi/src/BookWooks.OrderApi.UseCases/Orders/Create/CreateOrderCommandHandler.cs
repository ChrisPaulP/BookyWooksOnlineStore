namespace BookWooks.OrderApi.UseCases.Orders.Create;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
  private readonly ILogger<CreateOrderCommandHandler> _logger;
  private readonly IRepository<Order> _repository;

  public CreateOrderCommandHandler(ILogger<CreateOrderCommandHandler> logger, IRepository<Order> orderRepository) => (_logger, _repository) = (logger, orderRepository);

  public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling CreateOrderCommand for CustomerId: {CustomerId}", request.CustomerId);

    var orderResult = await request
        .CreateOrder()
        .AddOrderItems(request.OrderItems)
        .SaveOrder(_repository.AddAsync, _repository, cancellationToken);

    return orderResult.Match<CreateOrderResult>(
        order => order.OrderId,
        errors => new CreateOrderErrors(new Errors.DomainValidationErrors(errors.Errors)));
  }
}


