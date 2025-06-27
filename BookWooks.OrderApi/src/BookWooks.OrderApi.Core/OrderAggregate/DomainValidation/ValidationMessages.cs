namespace BookWooks.OrderApi.Core.OrderAggregate.DomainValidation;

public static class ValidationMessages
{
  // Entity Names
  public const string OrderId = "Order Id";
  public const string ProductId = "Product Id";
  public const string OrderItemId = "Order Item Id";
  public const string CustomerId = "Customer Id";

  // Order
  public const string OrderMessage = "Order Message";
  public const string DateTime = "Date Time";
  public const string DateInvalid = "{0} cannot be in the future."; 

  // Address
  public const string Street = "Street";
  public const string City = "City";
  public const string Country = "Country";
  public const string PostCode = "PostCode";

  // Product
  public const string ProductName = "Product Name";
  public const string ProductDescription = "Product Description";
  public const string ProductPrice = "Product Price";
  public const string ProductQuantity = "Product Quantity";

  // Customer
  public const string EmailAddress = "Email Address";
  public const string CustomerName = "Customer Name";
  public const string EmailInvalidFormat = "Invalid Email Address Format";
  public const string EmailInvalidDomain = "Email Address domain is not allowed";
  public const string EmailAddressErrors = "Email Address errors";

  // Payment
  public const string CardName = "Card Name";
  public const string CardNameInvalid = "Card Name must contain only letters, hyphens, periods, and apostrophes";
  public const string CardNumber = "Card Number";
  public const string CardNumberInvalid = "Card Number must contain only digits";
  public const string Expiration = "Expiration";
  public const string ExpirationInvalid = "Expiration date must be in MM/YY format";
  public const string PaymentMethod = "Payment Method";

  // Generic Validation
  public const string DoesNotMeetRequirements = "{0} does not meet requirements"; 
  public const string Required = "{0} is required"; 
  public const string TooLong = "{0} must be less than {1} characters"; 
  public const string TooShort = "{0} must be greater than {1} characters"; 
  public const string MustBeEqual = "{0} must be equal to {1} characters"; 
  public const string StreetInvalid = "Street must contain only letters, numbers, spaces, hyphens, and apostrophes";
  public const string PostCodeInvalid = "PostCode must be alphanumeric and can include spaces";

  // Numeric Validation
  public const string PriceInvalid = "Price must be greater than 0";
  public const string QuantityInvalid = "Quantity must be greater than 0";

  // Order Item
  public const string OrderItemNotFound = "Order item with ProductId '{0}' does not exist."; 
  public const string OrderItemNotFoundTitle = "Order item not found";
}

public static class EmailDomains
{
  public const string Gmail = "gmail.com";
  public const string Outlook = "outlook.com";
}
