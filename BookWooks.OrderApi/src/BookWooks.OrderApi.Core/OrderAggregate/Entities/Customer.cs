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
    var customerIdValidation = CustomerId.TryFrom(customerId).ToValidationMonad(errors => new CustomerValidationErrors(DomainValidationMessages.CustomerId, errors));
    var nameValidation = CustomerName.TryFrom(name).ToValidationMonad(errors => new CustomerValidationErrors(DomainValidationMessages.CustomerName, errors));
    var emailValidation = EmailAddress.TryFrom(email).ToValidationMonad(errors => new CustomerValidationErrors(DomainValidationMessages.EmailAddressErrors, errors));
    if (emailValidation.IsFail)
    {
      var customerValidationErrors = emailValidation.FailToList();
      foreach (var error in customerValidationErrors)
      {
        Console.WriteLine(error); // Or use error.ToString() if you want the full object
      }
    }
    return (customerIdValidation, nameValidation, emailValidation).Apply((createdCustomerId, createdName, createdEmail) =>
    {
      return new Customer(createdCustomerId, createdName, createdEmail);
    });
  }
}
