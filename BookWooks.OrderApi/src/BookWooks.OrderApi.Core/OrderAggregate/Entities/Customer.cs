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
  public static Validation<CustomerValidationErrors, Customer> CreateCustomer(string name, string email)
  {
    var maybeName = CustomerName.TryFrom(name).ToValidationMonad(errors => new CustomerValidationErrors("Customer Name", errors));
    var maybeEmail = EmailAddress.TryFrom(email).ToValidationMonad(errors => new CustomerValidationErrors("Email Address errors", errors));

    return (maybeName, maybeEmail).Apply((nameVo, emailVo) =>
    {
      return new Customer(CustomerId.New(), nameVo, emailVo);
    });
  }
}
