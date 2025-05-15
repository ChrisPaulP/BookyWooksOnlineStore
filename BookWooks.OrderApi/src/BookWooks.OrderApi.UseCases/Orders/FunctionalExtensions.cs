namespace BookWooks.OrderApi.UseCases.Orders;
public static class FunctionalExtensions
{
  public static ValidationErrors MapValidationErrors<T>(Seq<T> failures)
    where T : OrderValidationErrors
  {
    return failures.MapValidationErrorDetails(
        f => f.Match(
            customerErrors => ErrorType.Customer,
            deliveryAddressErrors => ErrorType.DeliveryAddress,
            paymentErrors => ErrorType.Payment,
            orderItemErrors => ErrorType.OrderItem,
            orderIdErrors => ErrorType.OrderId),
        f => f.Match(
            customerErrors => customerErrors.Errors.Select(e => ValidationError.Create(e.errorMessage)),
            deliveryAddressErrors => deliveryAddressErrors.Errors.Select(e => ValidationError.Create(e.errorMessage)),
            paymentErrors => paymentErrors.Errors.Select(e => ValidationError.Create(e.errorMessage)),
            orderItemErrors => orderItemErrors.Errors.Select(e => ValidationError.Create(e.errorMessage)),
            orderIdErrors => orderIdErrors.Errors.Select(e => ValidationError.Create(e.errorMessage)))
    );
  }

  public static ValidationErrors MapValidationErrorDetails<T>(
      this Seq<T> failures,
      Func<T, ErrorType> getErrorType,
      Func<T, IEnumerable<ValidationError>> getErrors)
  {
    return new ValidationErrors(
        failures
            .GroupBy(getErrorType)
            .ToDictionary(
                group => group.Key,
                group => group
                    .SelectMany(getErrors)
                    .ToList() as IReadOnlyList<ValidationError>)
    );
  }
    public static Either<TLeft, TRight> ToEither<TLeft, TRight>(this TRight? entity, Func<TLeft> leftValueFactory)
        where TRight : class
    {
      return entity != null
          ? Right(entity)
          : Left(leftValueFactory());
    }
  public static Either<TLeft, IEnumerable<TRight>> ToEither<TLeft, TRight>(
    this IEnumerable<TRight> source,
    Func<TLeft> leftValueFactory)
  {
    return source.Any()
        ? Right<TLeft, IEnumerable<TRight>>(source)
        : Left<TLeft, IEnumerable<TRight>>(leftValueFactory());
  }
  public static Either<ValidationErrors, Order> CreateOrder(this CreateOrderCommand request)
  {
    var deliveryAddress = request.DeliveryAddress;
    var paymentDetails = request.PaymentDetails;
    return Order.CreateOrder(
            request.CustomerId,
            deliveryAddress.Street,
            deliveryAddress.City,
            deliveryAddress.Country,
            deliveryAddress.Postcode,
            paymentDetails.CardHolderName,
            paymentDetails.CardNumber,
            paymentDetails.ExpiryDate,
            paymentDetails.PaymentMethod)
        .ToEither()
        .MapLeft(MapValidationErrors);
  }

  public static Either<ValidationErrors, Order> AddOrderItems(
      this Either<ValidationErrors, Order> eitherOrder,
      IEnumerable<CreateOrderItemCommand> orderItems) =>
      eitherOrder
          .Bind(order =>
              orderItems
                  .Aggregate(
                      Validation<OrderValidationErrors, Order>.Success(order),
                      (currentOrderOrderItemValidation, orderItem) =>
                          currentOrderOrderItemValidation.Bind(currentOrder =>
                              currentOrder.AddOrderItem(currentOrder.OrderId.Value, orderItem.ProductId, orderItem.Price, orderItem.ProductName, orderItem.ProductDescription, orderItem.Quantity)))
                  .ToEither()
                  .MapLeft(MapValidationErrors)
          );

  public static async Task<Either<ValidationErrors, Order>> SaveOrder(
        this Either<ValidationErrors, Order> eitherOrder,
        IRepository<Order> orderRepository,
        CancellationToken cancellationToken,
        ILogger logger)
    {
        return await eitherOrder.BindAsync(async order =>
        {
            logger.LogInformation("Saving order...");
            await orderRepository.AddAsync(order);
            await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            logger.LogInformation("Order saved successfully with Id: {OrderId}", order.OrderId);
            return Right<ValidationErrors, Order>(order);
        });
    }
  public static async Task<Either<TLeft, Order>> SaveOrder<TLeft>(
    this Either<TLeft, Order> either,
    Func<Order, Task> saveAction,
    IRepository<Order> repository,
    CancellationToken cancellationToken)
    where TLeft : class
  {
    return await either.BindAsync(async order =>
    {
      await saveAction(order);
      await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
      return Right<TLeft, Order>(order);
    });
  }

}

//.SaveOrder((repo, order) => repo.Update(order), _repository, cancellationToken)
