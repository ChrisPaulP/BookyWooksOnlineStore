namespace BookWooks.OrderApi.Core.OrderAggregate;
public class Order : EntityBase<Guid>, IAggregateRoot
{
  public OrderStatus Status { get; private set; } = OrderStatus.Pending;
  public int OrderTotal { get; private set; }
  public DateTime OrderPlaced { get; private set; }
  public bool OrderPaid { get; private set; }
  public bool IsCancelled { get; private set; }
  public string Message { get; private set; }
  public DeliveryAddress DeliveryAddress { get; private set; }

  public int? PaymentMethodId { get; private set; }
  public int? CustomerId { get; private set; }

  private readonly List<OrderItem> _orderItems;
  public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;
  protected Order()
  {
    _orderItems = new List<OrderItem>();
    Message = "New Order Created";
    IsCancelled = false;
    DeliveryAddress = new DeliveryAddress("","","","");
  }
  public Order(string userId, string userName, DeliveryAddress deliveryAddress, string cardNumber, string cardSecurityNumber,
           string cardHolderName, int? customerId = null, int? paymentMethodId = null) : this()
  {  
    OrderPlaced = DateTime.UtcNow;
    CustomerId = customerId;
    PaymentMethodId = paymentMethodId;
    DeliveryAddress = Guard.Against.Null(deliveryAddress, nameof(deliveryAddress));

    var orderCreatedDomainEvent = new OrderCreatedEvent(this);
    RegisterDomainEvent(orderCreatedDomainEvent);

  }
  public void AddOrderItem(int bookId, string bookTitle, decimal bookPrice, string bookImageUrl, int quantity = 1)
  {
    var orderItem = new OrderItem(bookId, bookTitle, bookPrice, bookImageUrl, quantity);
    _orderItems.Add(orderItem);
  }
  public decimal CalculateOrderTotal()
  {
    decimal total = 0;

    foreach (var item in OrderItems)
    {
      total += item.BookPrice * item.Quantity;
    }

    return total;
  }
  public void UpdateMessage(string newMessage)
  {
    Message = Guard.Against.NullOrEmpty(newMessage, nameof(newMessage));
  }
  public void OrderApproved()
  {
    Status = OrderStatus.Approved;
  }
  public void OrderCancelled()
  {
    Status = OrderStatus.Cancelled;
  }

  public void OrderBookStockConfirmed()
  {
    Status = OrderStatus.BookStockConfirmed;
  }
  public void OrderFulfilled()
  {
    Status = OrderStatus.Fulfilled;
    this.RegisterDomainEvent(new OrderFulfilledEvent(this));
  }
}


