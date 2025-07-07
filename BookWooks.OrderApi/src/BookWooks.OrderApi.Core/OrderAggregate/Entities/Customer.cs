namespace BookWooks.OrderApi.Core.OrderAggregate.Entities;
public record Customer : EntityBase
{
  public CustomerId CustomerId { get; set; }
  public CustomerName Name { get; private set; } 
  public EmailAddress Email { get; private set; }
  // Parameterless constructor for EF Core
  private Customer() 
  {
    CustomerId = CustomerId.New();
  }
  private Customer(CustomerId id, CustomerName name, EmailAddress email)
  {
    CustomerId = id;
    Name = name;
    Email = email;
  }
  public static Validation<CustomerValidationErrors, Customer> CreateCustomer(Guid customerId, string name, string email)
  {
    var customerIdValidation = CustomerId.TryFrom(customerId).ToValidationMonad(errors => new CustomerValidationErrors(ValidationMessages.CustomerId, errors));
    var nameValidation = CustomerName.TryFrom(name).ToValidationMonad(errors => new CustomerValidationErrors(ValidationMessages.CustomerName, errors));
    var emailValidation = EmailAddress.TryFrom(email).ToValidationMonad(errors => new CustomerValidationErrors(ValidationMessages.EmailAddressErrors, errors));

    return (customerIdValidation, nameValidation, emailValidation).Apply((createdCustomerId, createdName, createdEmail) =>
    {
      return new Customer(createdCustomerId, createdName, createdEmail);
    });
  }
}
