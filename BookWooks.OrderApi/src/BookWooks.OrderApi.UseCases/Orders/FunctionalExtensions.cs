namespace BookWooks.OrderApi.UseCases.Orders;

/// <summary>
/// Extension methods for functional programming patterns in the order domain
/// </summary>
public static class FunctionalExtensions
{
  /// <summary>
  /// Maps domain validation errors to a structured validation error object
  /// </summary>
  /// <param name="failures">Collection of validation failures</param>
  /// <returns>Structured validation errors grouped by error type</returns>
  public static DomainValidationErrors MapValidationErrors(Seq<OrderDomainValidationErrors> failures)
  {
    return failures.MapValidationErrorDetails(
        GetErrorType,
        MapErrorMessages);
  }

  private static ErrorType GetErrorType(OrderDomainValidationErrors failure) =>
      failure.Match(
          orderErrors => ErrorType.Order,
          customerErrors => ErrorType.Customer,
          deliveryAddressErrors => ErrorType.DeliveryAddress,
          paymentErrors => ErrorType.Payment,
          orderItemErrors => ErrorType.OrderItem,
          orderIdErrors => ErrorType.OrderId,
          orderStatusValidationError => ErrorType.OrderStatusValidationError);

  private static IEnumerable<DomainValidationError> MapErrorMessages(OrderDomainValidationErrors failure) =>
      failure.Match(
          orderErrors => orderErrors.Errors.Select(e => DomainValidationError.Create(e.errorMessage)),
          customerErrors => customerErrors.Errors.Select(e => DomainValidationError.Create(e.errorMessage)),
          deliveryAddressErrors => deliveryAddressErrors.Errors.Select(e => DomainValidationError.Create(e.errorMessage)),
          paymentErrors => paymentErrors.Errors.Select(e => DomainValidationError.Create(e.errorMessage)),
          orderItemErrors => orderItemErrors.Errors.Select(e => DomainValidationError.Create(e.errorMessage)),
          orderIdErrors => orderIdErrors.Errors.Select(e => DomainValidationError.Create(e.errorMessage)),
          orderStatusValidationError => new[] { DomainValidationError.Create(orderStatusValidationError.Error.errorMessage) });

  /// <summary>
  /// Maps validation errors to a structured format grouped by error type
  /// </summary>
  public static DomainValidationErrors MapValidationErrorDetails<T>(
      this Seq<T> failures,
      Func<T, ErrorType> getErrorType,
      Func<T, IEnumerable<DomainValidationError>> getErrors)
  {
    var errorsByType = failures
        .GroupBy(getErrorType)
        .ToDictionary(
            group => group.Key,
            group => group.SelectMany(getErrors).ToList() as IReadOnlyList<DomainValidationError>);

    return new DomainValidationErrors(errorsByType);
  }

  /// <summary>
  /// Converts a nullable entity to an Either type
  /// </summary>
  public static Either<TLeft, TRight> ToEither<TLeft, TRight>(
      this TRight? entity,
      Func<TLeft> leftValueFactory)
      where TRight : class
  {
    return entity != null
        ? Right(entity)
        : Left(leftValueFactory());
  }

  /// <summary>
  /// Converts an enumerable to an Either type, returning Left if empty
  /// </summary>
  public static Either<TLeft, IEnumerable<TRight>> ToEither<TLeft, TRight>(
      this IEnumerable<TRight> source,
      Func<TLeft> leftValueFactory)
  {
    return source.Any()
        ? Right<TLeft, IEnumerable<TRight>>(source)
        : Left<TLeft, IEnumerable<TRight>>(leftValueFactory());
  }

  /// <summary>
  /// Converts an order status validation to an Either type
  /// </summary>
  public static Either<OrderErrors, T> ToEither<T>(
      this Validation<OrderStatusValidationError, T> validation,
      Func<OrderStatusValidationError, OrderErrors> mapDomainError)
  {
    return validation.Match(
        success => Either<OrderErrors, T>.Right(success),
        error => Either<OrderErrors, T>.Left(mapDomainError(error.First()))
    );
  }

  /// <summary>
  /// Creates an order from a command with validation
  /// </summary>
  public static Either<DomainValidationErrors, Order> CreateOrder(this CreateOrderCommand request)
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

  /// <summary>
  /// Adds order items to an order with validation
  /// </summary>
  public static Either<DomainValidationErrors, Order> AddOrderItems(
      this Either<DomainValidationErrors, Order> eitherOrder,
      IEnumerable<CreateOrderItemCommand> orderItems) =>
        eitherOrder.Bind(order =>
            orderItems.Aggregate(
                Validation<OrderDomainValidationErrors, Order>.Success(order),
                AddItem
            )
            .ToEither()
            .MapLeft(MapValidationErrors));

  private static Validation<OrderDomainValidationErrors, Order> AddItem(
      Validation<OrderDomainValidationErrors, Order> validation,
      CreateOrderItemCommand orderItem) =>
       validation.Bind(order =>
          order.AddOrderItem(
              order.OrderId.Value,
              orderItem.ProductId,
              orderItem.Price,
              orderItem.ProductName,
              orderItem.ProductDescription,
              orderItem.Quantity));

  /// <summary>
  /// Saves an order with validation
  /// </summary>
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
