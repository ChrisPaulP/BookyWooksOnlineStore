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
            orderItemErrors => ErrorType.OrderItem),
        f => f.Match(
            customerErrors => customerErrors.Errors.Select(e => ValidationError.Create(e.errorMessage)),
            deliveryAddressErrors => deliveryAddressErrors.Errors.Select(e => ValidationError.Create(e.errorMessage)),
            paymentErrors => paymentErrors.Errors.Select(e => ValidationError.Create(e.errorMessage)),
            orderItemErrors => orderItemErrors.Errors.Select(e => ValidationError.Create(e.errorMessage)))
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
  public static Either<Error, IEnumerable<T>> ToEither<T>(this IEnumerable<T> source, Error leftValue)
  {
    return source.Any() ? Right<Error, IEnumerable<T>>(source) : Left<Error, IEnumerable<T>>(leftValue);
  }
  public static Either<TLeft, Seq<T>> ToEither<TLeft, T>(this IEnumerable<T> source, Func<TLeft> leftValueFactory)
    where TLeft : Error
  {
    var seq = source.ToSeq();
    return seq.IsEmpty ? Left<TLeft, Seq<T>>(leftValueFactory()) : Right<TLeft, Seq<T>>(seq);
  }
  public static Option<T> ToOption<T>(this T? value) where T : class
  {
    return value == null ? Option<T>.None : Option<T>.Some(value);
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
      IEnumerable<OrderItem> orderItems)
  {
    return eitherOrder.Bind(order =>
        orderItems.Aggregate(
            Validation<OrderValidationErrors, Order>.Success(order),
            (currentOrderValidation, item) =>
                currentOrderValidation.Bind(currentOrder =>
                    currentOrder.AddOrderItem(item.ProductId, item.Price, item.ProductName, item.ProductDescription, item.Quantity)))
        .ToEither()
        .MapLeft(MapValidationErrors));
  }
  public static Either<ValidationErrors, Order> AddOrderItemsAlternative(
        this Either<ValidationErrors, Order> eitherOrder,
        IEnumerable<OrderItem> orderItems)
    {
        return eitherOrder.Bind(order =>
            orderItems
                .Select<OrderItem, Func<Order, Validation<OrderValidationErrors, Order>>>(item => (Order o) => o.AddOrderItem(
                    item.ProductId, item.Price, item.ProductName, item.ProductDescription, item.Quantity))
                .Aggregate(Validation<OrderValidationErrors, Order>.Success(order), (validOrder, addItem) => validOrder.Bind(addItem))
                .ToEither().MapLeft(MapValidationErrors)
        );
    }

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
            logger.LogInformation("Order saved successfully with Id: {OrderId}", order.Id);
            return Prelude.Right<ValidationErrors, Order>(order);
        });
    }
}


