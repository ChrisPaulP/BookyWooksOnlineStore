using ValidationExtensions = BookWooks.OrderApi.UseCases.FunctionalExtensions.ValidationExtensions;

namespace BookWooks.OrderApi.UseCases.Orders;

/// <summary>
/// Contains order-specific operations using functional programming patterns
/// </summary>
public static class FunctionalOrderOperations
{
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
            .MapLeft(ValidationExtensions.MapValidationErrors);
    }

  /// <summary>
  /// Adds order items to an order with validation
  /// </summary>
  public static Either<DomainValidationErrors, Order> AddOrderItems(this Either<DomainValidationErrors, Order> eitherOrder, IEnumerable<CreateOrderItemCommand> orderItems) =>
        eitherOrder.Bind(order =>
            orderItems.Aggregate(
                Validation<OrderDomainValidationErrors, Order>.Success(order),
                AddItem
            )
            .ToEither()
            .MapLeft(ValidationExtensions.MapValidationErrors));

  private static Validation<OrderDomainValidationErrors, Order> AddItem(Validation<OrderDomainValidationErrors, Order> validation, CreateOrderItemCommand orderItem) =>
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
