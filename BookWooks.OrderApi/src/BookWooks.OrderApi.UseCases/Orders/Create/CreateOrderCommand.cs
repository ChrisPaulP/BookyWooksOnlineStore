
namespace BookWooks.OrderApi.UseCases.Create;
public record CreateOrderCommand(Guid CustomerId, Address DeliveryAddress, PaymentDetails PaymentDetails, IEnumerable<OrderItem> OrderItems) : CommandBase<Either<ValidationErrors, Guid>>; 
public record OrderItem(Guid ProductId, string ProductName, string ProductDescription, decimal Price,  int Quantity);
public record Address(string Street, string City, string Country, string Postcode);
public record PaymentDetails(string CardNumber, string CardHolderName, string ExpiryDate, int PaymentMethod);

