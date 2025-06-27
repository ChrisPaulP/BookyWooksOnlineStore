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
        OrderItems = ImmutableList<OrderItem>.Empty;
    }
    public OrderId OrderId { get; private set; } 
    public CustomerId CustomerId { get; private set; }
    public DeliveryAddress DeliveryAddress { get; private set; }
    public Payment Payment { get; private set; }
    public OrderStatus Status { get; private set; }
    public OrderPlaced OrderPlaced { get; private set; }
    public Message Message { get; private set; }
    public IsCancelled IsCancelled { get; private set; }
    public ImmutableList<OrderItem> OrderItems { get; private set; } 

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
        OrderItems = orderItems;
    }

    public static Validation<OrderValidationErrors, Order> CreateOrder(Guid customerId, string street, string city, string country, string postcode, string cardHolderName, string cardNumber, string expiration, int paymentMethod)
    {
        var orderId = OrderId.New();
        var orderItems = ImmutableList<OrderItem>.Empty;

        var addressValidation = DeliveryAddress.TryCreate(street, city, country, postcode);
        var paymentValidation = Payment.TryCreate(cardHolderName, cardNumber, expiration, paymentMethod);

        var messageValidation = Message.TryFrom(OrderStatus.NewOrderCreated.Label).ToValidationMonad<OrderValidationErrors, Message>(errors => new OrderErrors(ValidationMessages.OrderMessage, errors));
        var customerIdValidation = CustomerId.TryFrom(customerId).ToValidationMonad<OrderValidationErrors, CustomerId>(errors => new CustomerIdErrors(ValidationMessages.CustomerId, errors));

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
        return orderItem == null
            ? new OrderItemValidationErrors(ValidationMessages.OrderItemNotFoundTitle,[BusinessRuleError.Create(string.Format(ValidationMessages.OrderItemNotFound, productId))])
            : orderItem.UpdateQuantity(newQuantity);
    }

    public decimal OrderTotal => OrderItems.Sum(item => item.Price.Value * item.Quantity.Value);

    public Order ApproveOrder() => this with { Status = OrderStatus.Approved };

    public Order CancelOrder() => this with { Status = OrderStatus.Cancelled, IsCancelled = IsCancelled.True };

    public Order ConfirmBookStock() => this with { Status = OrderStatus.BookStockConfirmed };

    public Order FulfillOrder()
    {
        this.RegisterDomainEvent(new OrderFulfilledEvent(this));
        return this with { Status = OrderStatus.Fulfilled };
    }

    public Order UpdateMessage(string newMessage) => this with { Message = Message.From(newMessage) };

    public Validation<OrderValidationErrors, Order> AddOrderItem(Guid orderId, Guid productId, decimal price, string productName, string productDescription, int quantity = 1)
    {
        return OrderItem.AddOrderItem(orderId, productId, price, quantity, productName, productDescription)
            .Map(orderItem => this with { OrderItems = OrderItems.Add(orderItem) })
            .MapFail(orderItemErrors => new OrderValidationErrors(orderItemErrors));
    }
}
