
namespace BookWooks.OrderApi.UseCases.Create;
public record CreateOrderCommand(Guid CustomerId, CreateAddressCommand DeliveryAddress, CreatePaymentDetailsCommand PaymentDetails, IEnumerable<CreateOrderItemCommand> OrderItems) : CommandBase<CreateOrderResult>; 
public record CreateOrderItemCommand(Guid ProductId, string ProductName, string ProductDescription, decimal Price,  int Quantity);
public record CreateAddressCommand(string Street, string City, string Country, string Postcode);
public record CreatePaymentDetailsCommand(string CardNumber, string CardHolderName, string ExpiryDate, int PaymentMethod);

