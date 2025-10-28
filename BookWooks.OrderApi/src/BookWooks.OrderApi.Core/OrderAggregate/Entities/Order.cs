namespace BookWooks.OrderApi.Core.OrderAggregate.Entities;

public record Order : EntityBase, IAggregateRoot
{
  // Required for EF Core
  private Order()
    {
        OrderId = OrderId.New();
        CustomerId = CustomerId.New();
        DeliveryAddress = default!;
        Payment = default!;
        Status = default!;
    }
    public OrderId OrderId { get; private set; } 
    public CustomerId CustomerId { get; private set; }
    public DeliveryAddress DeliveryAddress { get; private set; }
    public Payment Payment { get; private set; }
    public OrderStatus Status { get; private set; }
    public OrderPlaced OrderPlaced { get; private set; }
    public Message Message { get; private set; }
    public IsCancelled IsCancelled { get; private set; }
    // Backing field for EF Core
    private readonly List<OrderItem> _orderItems = new();
    public ImmutableList<OrderItem> OrderItems => _orderItems.ToImmutableList();
  // Cache for order total - not readonly since it needs to be updated
  private decimal? _cachedOrderTotal;

  private Order(OrderId orderId, CustomerId customerId, DeliveryAddress deliveryAddress, Payment payment,OrderStatus status, OrderPlaced orderPlaced, Message message, IsCancelled isCancelled, ImmutableList<OrderItem> orderItems)
    {
        OrderId = orderId;
        CustomerId = customerId;
        DeliveryAddress = deliveryAddress;
        Payment = payment;
        Status = status;
        OrderPlaced = orderPlaced;
        Message = message;
        IsCancelled = isCancelled;
        _orderItems = orderItems.ToList();
    }

    public static Validation<OrderDomainValidationErrors, Order> CreateOrder(Guid customerId, string street, string city, string country, string postcode, string cardHolderName, string cardNumber, string expiration, int paymentMethod)
    {
        var orderId = OrderId.New();
        var orderItems = ImmutableList<OrderItem>.Empty;

        var addressValidation = DeliveryAddress.TryCreate(street, city, country, postcode);
        var paymentValidation = Payment.TryCreate(cardHolderName, cardNumber, expiration, paymentMethod);

        var messageValidation = Message.TryFrom(OrderStatus.NewOrderCreated.Label).ToValidationMonad((Func<IReadOnlyList<BusinessRuleError>, OrderDomainValidationErrors>)(errors => new OrderValidationErrors(DomainValidationMessages.OrderMessage, errors)));
        var customerIdValidation = CustomerId.TryFrom(customerId).ToValidationMonad<OrderDomainValidationErrors, CustomerId>(errors => new CustomerIdValidationErrors(DomainValidationMessages.CustomerId, errors));

        return (addressValidation, paymentValidation, messageValidation, customerIdValidation).Apply((createdAddress, createdPayment, createdMessage, createdCustomerId) =>
        {
            var order = new Order(orderId, createdCustomerId, createdAddress, createdPayment, OrderStatus.Pending, OrderPlaced.From(DateTime.UtcNow), createdMessage, IsCancelled.False, orderItems);
            order.RegisterDomainEvent(new OrderCreatedDomainEvent(order));
            return order;
        });
    }

  public Validation<OrderItemValidationErrors, OrderItem> UpdateOrderItemQuantity(Guid productId, int newQuantity)
  {
    var orderItem = OrderItems.FirstOrDefault(item => item.ProductId.Value == productId);
    return orderItem == null ?
    
       new OrderItemValidationErrors(DomainValidationMessages.OrderItemNotFoundTitle, [BusinessRuleError.Create(string.Format(DomainValidationMessages.OrderItemNotFound, productId))])
     : orderItem.UpdateQuantity(newQuantity)
        .Map(updatedItem =>
        {
          var index = _orderItems.FindIndex(i => i.ProductId.Value == productId);
          _orderItems[index] = updatedItem;
          _cachedOrderTotal = null; // Invalidate cache
          return updatedItem;
        });
    }


      public decimal OrderTotal
      {
        get
        {
          if (!_cachedOrderTotal.HasValue)
          {
            _cachedOrderTotal = OrderItems.Sum(item => item.Price.Value * item.Quantity.Value);
          }
          return _cachedOrderTotal.Value;
        }
      }
    private Validation<OrderStatusValidationError, bool> ValidateStateTransition(OrderStatus newStatus)
    {
      if (IsCancelled.Value && newStatus != OrderStatus.Cancelled)
      {
        return new OrderStatusValidationError("Invalid State Transition",
            BusinessRuleError.Create("Cannot transition a cancelled order"));
      }

      // Add specific state transition rules
      if (Status == OrderStatus.Fulfilled &&
          (newStatus == OrderStatus.Pending || newStatus == OrderStatus.BookStockConfirmed))
      {
        return new OrderStatusValidationError("Invalid State Transition",
            BusinessRuleError.Create($"Cannot transition from {Status.Label} to {newStatus.Label}"));
      }

      return true;
    }
  public Validation<OrderStatusValidationError, Order> ApproveOrder() => ValidateStateTransition(OrderStatus.Approved).Map(_ => this with { Status = OrderStatus.Approved });

  public Validation<OrderStatusValidationError, Order> CancelOrder() => ValidateStateTransition(OrderStatus.Cancelled).Map(_ => this with { Status = OrderStatus.Cancelled, IsCancelled = IsCancelled.True });

  public Validation<OrderStatusValidationError, Order> ConfirmBookStock() => ValidateStateTransition(OrderStatus.BookStockConfirmed) .Map(_ => this with { Status = OrderStatus.BookStockConfirmed });

  public Validation<OrderStatusValidationError, Order> FulfillOrder() => ValidateStateTransition(OrderStatus.Fulfilled)
     .Map(_ =>
          {
            var order = this with { Status = OrderStatus.Fulfilled };
            order.RegisterDomainEvent(new OrderFulfilledDomainEvent(order));
            return order;
          });

  public Order UpdateMessage(string newMessage) => this with { Message = Message.From(newMessage) };

  public Validation<OrderDomainValidationErrors, Order> AddOrderItem(Guid orderId, Guid productId,
      decimal price, string productName, string productDescription, int quantity = 1)
  {
    return OrderItem.AddOrderItem(orderId, productId, price, quantity, productName, productDescription)
        .Map(orderItem =>
        {
          _orderItems.Add(orderItem);
          _cachedOrderTotal = null; 
          return this;
        })
        .MapFail(orderItemErrors => new OrderDomainValidationErrors(
            orderItemErrors));
  }
  public static readonly string OrderItemsFieldName = "_orderItems";
}
