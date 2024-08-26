namespace BookWooks.OrderApi.UseCases.Create;
public record CreateOrderCommand(IEnumerable<OrderItem> OrderItems, Guid CustomerId, Address DeliveryAddress, PaymentDetails PaymentDetails) : ICommand<DetailedResult<Guid>>;
public record OrderItem(Guid ProductId, decimal Price,  int Quantity);
public record Address(string Street, string City, string Country, string Postcode);
public record PaymentDetails(string CardNumber, string CardHolderName, string ExpiryDate, string Cvv, int PaymentMethod);

