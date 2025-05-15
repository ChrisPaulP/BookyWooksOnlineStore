
using BookWooks.OrderApi.Core.OrderAggregate.BusinessRuleErrors;

namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;


[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public sealed partial record CustomerId
{
    public static Validation Validate( Guid customerId)
    {
      var result = Validation.Invalid("Product Id is missing");

      var isNull = customerId == Guid.Empty;
      if (isNull) result.WithData(BusinessRuleError.Create("Product Id is required"), string.Empty);

      return result.Data is { Count: > 0 } ? result: Vogen.Validation.Ok;
    }
  public static CustomerId New()
  {
    return From(Guid.NewGuid());
  }

  //public bool Equals(CustomerId other) // ⚠ No "override" needed in record struct!
  //{
  //  Console.WriteLine($"🔍 Comparing OrderId: {Value} with {other.Value}");

  //  if (!IsInitialized() || !other.IsInitialized())
  //  {
  //    Console.WriteLine("❌ One of the OrderId instances is not initialized!");
  //    return false;
  //  }

  //  return Value.Equals(other.Value);
  //}
}

  [ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
  public readonly partial record struct CustomerName
  {
    private static Validation Validate(string input)
    {
      var result = Validation.Invalid("Customer name does not meet requirements");
      if (string.IsNullOrWhiteSpace(input)) result.WithData(BusinessRuleError.Create("Customer Name is required"), string.Empty);
      if (input.Length > 50) result.WithData(BusinessRuleError.Create("Customer Name must be less than 50 characters"), string.Empty);
      return result.Data is { Count: > 0 } ? result : Validation.Ok;
    }
  }

  [ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
  public readonly partial record struct EmailAddress
  {
  private static Validation Validate(string input)
  {
    var result = Validation.Invalid("Email Address does not meet requirements");
    if (string.IsNullOrWhiteSpace(input)) result.WithData(BusinessRuleError.Create("Email Address is required"), string.Empty);
    if (input.Length > 500) result.WithData(BusinessRuleError.Create("Email Address must be less than 500 characters"), string.Empty);
    if (!IsValidEmailFormat(input)) result.WithData(BusinessRuleError.Create("Email Address format is invalid"), string.Empty);
    if (!IsAllowedDomain(input)) result.WithData(BusinessRuleError.Create("Email Address domain is not allowed"), string.Empty);
    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }
  private static bool IsValidEmailFormat(string email)
  {
    var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    return emailRegex.IsMatch(email);
  }
   private static bool IsAllowedDomain(string email)
  {
    var allowedDomains = new[] { "gmail.com", "example.com", "anotherdomain.com" };
    var emailDomain = email.Split('@').Last();
    return allowedDomains.Contains(emailDomain);
  }
}



