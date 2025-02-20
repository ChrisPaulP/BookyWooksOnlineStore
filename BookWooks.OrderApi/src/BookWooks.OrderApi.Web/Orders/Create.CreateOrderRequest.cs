namespace BookWooks.OrderApi.Web.Orders;

public record CreateOrderRequest(
    Guid Id,
    Guid CustomerId,
    OrderRequestAddress Address,
    OrderRequestPayment Payment,
    List<OrderRequestOrderItem> OrderItems
)
{
  public const string Route = "/Orders";
}

public record OrderRequestOrderItem(
    Guid ProductId,
    decimal Price,
    int Quantity = 1,
    string ProductName = "",
    string ProductDescription = ""
    
);

public record OrderRequestAddress(
    string Street = "",
    string City = "",
    string Country = "",
    string Postcode = ""
);

public record OrderRequestPayment(
    string CardHolderName = "",
    string CardHolderNumber = "",
    string ExpiryDate = "",
    string Cvv = "",
    int PaymentMethod = 1
);


