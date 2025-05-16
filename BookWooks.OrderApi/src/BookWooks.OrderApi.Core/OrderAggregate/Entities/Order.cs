using BookyWooks.Messaging.Messages.InitialMessage;
using BookyWooks.SharedKernel;

namespace BookWooks.OrderApi.Core.OrderAggregate.Entities;

//public class Order : EntityBase, IAggregateRoot
//{
//  private Order() { } 
//  public OrderId OrderId { get; private set; } = OrderId.New();
//  public CustomerId CustomerId { get; private set; }
//  public DeliveryAddress DeliveryAddress { get; private set; } = default!;
//  public Payment Payment { get; private set; } = default!;
//  public OrderStatus Status { get; private set; } = default!;
//  public OrderPlaced OrderPlaced { get; private set; }
//  public Message Message { get; private set; }
//  public IsCancelled IsCancelled { get; private set; }
//  public ImmutableList<OrderItem> OrderItems { get; private set; } = ImmutableList<OrderItem>.Empty;

//  private Order(OrderId orderId, CustomerId customerId, DeliveryAddress deliveryAddress, Payment payment,
//                OrderStatus status, OrderPlaced orderPlaced, Message message,
//                IsCancelled isCancelled, ImmutableList<OrderItem> orderItems)
//  {
//    OrderId = orderId;
//    CustomerId = customerId;
//    DeliveryAddress = deliveryAddress;
//    Payment = payment;
//    Status = status;
//    OrderPlaced = orderPlaced;
//    Message = message;
//    IsCancelled = isCancelled;
//    OrderItems = orderItems;
//  }

//  public static Validation<OrderValidationErrors, Order> CreateOrder(
//      Guid customerId, string street, string city, string country, string postcode,
//      string cardHolderName, string cardNumber, string expiration, int paymentMethod)
//  {
//    var orderId = OrderId.New();
//    var orderItems = ImmutableList<OrderItem>.Empty;
//    var mayBeCustomerId = CustomerId.TryFrom(customerId).ToValidationMonad<OrderValidationErrors, CustomerId>(errors => new CustomerIdErrors("CustomerId", errors));
//    var address = DeliveryAddress.TryCreate(street, city, country, postcode);
//    var payment = Payment.TryCreate(cardHolderName, cardNumber, expiration, paymentMethod);

//    if (address.IsFail || payment.IsFail)
//    {
//      Console.WriteLine("Invalid address or payment details");
//    }

//    return (mayBeCustomerId, address, payment).Apply((customervo, addressVo, paymentVo) =>
//    {
//      var order = new Order(orderId, customervo, addressVo, paymentVo, OrderStatus.Pending, OrderPlaced.From(DateTime.UtcNow), Message.From("New Order Created"), IsCancelled.False, orderItems);
//      return order;
//    });
//  }

//  public Validation<OrderItemValidationErrors, OrderItem> UpdateOrderItemQuantity(Guid productId, int newQuantity)
//  {
//    var orderItem = OrderItems.FirstOrDefault(item => item.ProductId == productId);
//    return orderItem == null
//        ? new OrderItemValidationErrors("Failed to Find Order Item", new List<BusinessRuleError> { BusinessRuleError.Create("Order Item does not exist") })
//        : orderItem.UpdateQuantity(newQuantity);
//  }

//  public decimal OrderTotal => OrderItems.Sum(item => item.Price.Value * item.Quantity.Value);

//  public Order ApproveOrder()
//  {
//    return new Order(OrderId, CustomerId, DeliveryAddress, Payment, OrderStatus.Approved, OrderPlaced, Message, IsCancelled, OrderItems);
//  }

//  public Order CancelOrder()
//  {
//    return new Order(OrderId, CustomerId, DeliveryAddress, Payment, OrderStatus.Cancelled, OrderPlaced, Message, IsCancelled.From(true), OrderItems);
//  }

//  public Order ConfirmBookStock()
//  {
//    return new Order(OrderId, CustomerId, DeliveryAddress, Payment, OrderStatus.BookStockConfirmed, OrderPlaced, Message, IsCancelled, OrderItems);
//  }

//  public Order FulfillOrder()
//  {
//    this.RegisterDomainEvent(new OrderFulfilledEvent(this));
//    return new Order(OrderId, CustomerId, DeliveryAddress, Payment, OrderStatus.Fulfilled, OrderPlaced, Message, IsCancelled, OrderItems);
//  }

//  public Order UpdateMessage(string newMessage)
//  {
//    return new Order(OrderId, CustomerId, DeliveryAddress, Payment, Status, OrderPlaced, Message.From(newMessage), IsCancelled, OrderItems);
//  }

//  public Validation<OrderValidationErrors, Order> AddOrderItem(Guid orderId, Guid productId, decimal price, string productName, string productDescription, int quantity = 1)
//  {
//    return OrderItem.AddOrderItem(orderId, productId, price, quantity, productName, productDescription)
//        .Map(orderItem => new Order(OrderId, CustomerId, DeliveryAddress, Payment, Status, OrderPlaced, Message, IsCancelled, OrderItems.Add(orderItem)))
//        .MapFail(orderItemErrors => new OrderValidationErrors(orderItemErrors));
//  }
//}
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
    public OrderId OrderId { get; private set; } //= OrderId.New();
    public CustomerId CustomerId { get; private set; }
    public DeliveryAddress DeliveryAddress { get; private set; } //= default!;
    public Payment Payment { get; private set; }// = default!;
    public OrderStatus Status { get; private set; }// = default!;
    public OrderPlaced OrderPlaced { get; private set; }
    public Message Message { get; private set; }
    public IsCancelled IsCancelled { get; private set; }
    public ImmutableList<OrderItem> OrderItems { get; private set; } //= ImmutableList<OrderItem>.Empty;

    private Order(OrderId orderId, CustomerId customerId, DeliveryAddress deliveryAddress, Payment payment,
                    OrderStatus status, OrderPlaced orderPlaced, Message message,
                    IsCancelled isCancelled, ImmutableList<OrderItem> orderItems)
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

    public static Validation<OrderValidationErrors, Order> CreateOrder(
        Guid customerId, string street, string city, string country, string postcode,
        string cardHolderName, string cardNumber, string expiration, int paymentMethod)
    {
        var orderId = OrderId.New();
        var orderItems = ImmutableList<OrderItem>.Empty;
        var mayBeCustomerId = CustomerId.TryFrom(customerId).ToValidationMonad<OrderValidationErrors, CustomerId>(errors => new CustomerIdErrors("CustomerId", errors));
        var address = DeliveryAddress.TryCreate(street, city, country, postcode);
        var payment = Payment.TryCreate(cardHolderName, cardNumber, expiration, paymentMethod);

        if (address.IsFail || payment.IsFail)
        {
            Console.WriteLine("Invalid address or payment details");
        }

        return (mayBeCustomerId, address, payment).Apply((customervo, addressVo, paymentVo) =>
        {
            var order = new Order(orderId, customervo, addressVo, paymentVo, OrderStatus.Pending, OrderPlaced.From(DateTime.UtcNow), Message.From("New Order Created"), IsCancelled.False, orderItems);
            order.RegisterDomainEvent(new OrderCreatedDomainEvent(order));
            return order;
        });
    }

    public Validation<OrderItemValidationErrors, OrderItem> UpdateOrderItemQuantity(Guid productId, int newQuantity)
    {
        var orderItem = OrderItems.FirstOrDefault(item => item.ProductId == productId);
        return orderItem == null
            ? new OrderItemValidationErrors("Failed to Find Order Item", new List<BusinessRuleError> { BusinessRuleError.Create("Order Item does not exist") })
            : orderItem.UpdateQuantity(newQuantity);
    }

    public decimal OrderTotal => OrderItems.Sum(item => item.Price.Value * item.Quantity.Value);

    public Order ApproveOrder() => this with { Status = OrderStatus.Approved };

    public Order CancelOrder() => this with { Status = OrderStatus.Cancelled, IsCancelled = IsCancelled.From(true) };

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
