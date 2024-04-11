namespace BookWooks.OrderApi.UseCases.Contributors.Create;
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Result<Guid>>
{
  private readonly ILogger<CreateOrderCommandHandler> _logger;
  private readonly IRepository<Order> _orderRepository;

  public CreateOrderCommandHandler(ILogger<CreateOrderCommandHandler> logger, IRepository<Order> orderRepository)
  {
    _logger = logger;
    _orderRepository = orderRepository;
  }

  public async Task<Result<Guid>> Handle(CreateOrderCommand request,
    CancellationToken cancellationToken)
  {
    var deliveryAddress = DeliveryAddress.Of(request.DeliveryAddress.Street, request.DeliveryAddress.City, request.DeliveryAddress.Country, request.DeliveryAddress.Postcode);
    var paymentDetails = Payment.Of(request.PaymentDetails.CardHolderName, request.PaymentDetails.CardNumber, request.PaymentDetails.ExpiryDate, request.PaymentDetails.Cvv, request.PaymentDetails.PaymentMethod);
    var newOrder = Order.Create(request.CustomerId, deliveryAddress, paymentDetails);
    if (newOrder == null)
      throw new NotFoundException("", "");
    foreach (var item in request.OrderItems)
    {
      newOrder.AddOrderItem(item.ProductId, item.Price, item.Quantity);
    }
    _logger.LogInformation("----- Creating Order - Order: {@Order}", newOrder);
    
    await _orderRepository.AddAsync(newOrder);
    await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    return Result.Success(newOrder.Id);
  }
}

