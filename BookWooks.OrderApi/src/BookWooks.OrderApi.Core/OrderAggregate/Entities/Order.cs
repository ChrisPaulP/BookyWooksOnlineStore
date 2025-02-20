using BookWooks.OrderApi.Core.OrderAggregate.BusinessRuleErrors;
using BookyWooks.SharedKernel.Validation;

namespace BookWooks.OrderApi.Core.OrderAggregate.Entities;
public record Order : EntityBase, IAggregateRoot
{
  public CustomerId CustomerId { get; }
  public DeliveryAddress DeliveryAddress { get; }
  public Payment Payment { get; }
  public OrderStatus Status { get; private set; }
  public OrderPlaced OrderPlaced { get; }
  public Message Message { get; private set; }
  public IsCancelled IsCancelled { get; private set; }
  public ImmutableList<OrderItem> OrderItems { get; private set; }

  private Order(CustomerId customerId, DeliveryAddress deliveryAddress, Payment payment,
                OrderStatus status, OrderPlaced orderPlaced, Message message,
                IsCancelled isCancelled, ImmutableList<OrderItem> orderItems)
  {
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

    var orderItems = ImmutableList<OrderItem>.Empty;
    var mayBeCustomerId = CustomerId.TryFrom(customerId).ToValidationMonad<OrderValidationErrors, CustomerId>(errors => new CustomerIdErrors("CustomerId", errors));
    var address = DeliveryAddress.TryCreate(street, city, country, postcode);
    var payment = Payment.TryCreate(cardHolderName, cardNumber, expiration, paymentMethod);

    return (mayBeCustomerId, address, payment).Apply((customervo, addressVo, paymentVo) =>
    {
      var order = new Order(customervo, addressVo, paymentVo, OrderStatus.Pending, OrderPlaced.From(DateTime.UtcNow), Message.From("New Order Created"), IsCancelled.False, orderItems);
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

  public Validation<OrderValidationErrors, Order> AddOrderItem(Guid productId, decimal price, string productName, string productDescription, int quantity = 1)
  {
    return OrderItem.AddOrderItem(Id, productId, price, quantity, productName, productDescription)
        .Map(orderItem => this with { OrderItems = OrderItems.Add(orderItem) })
        .MapFail(orderItemErrors => new OrderValidationErrors(orderItemErrors));
  }
}
