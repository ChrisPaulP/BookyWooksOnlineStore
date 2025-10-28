namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public sealed partial record CustomerId
{
  public static Validation Validate( Guid customerId) => ValueObjectValidation.ValidateGuid(customerId, DomainValidationMessages.CustomerId);
  public static CustomerId New() => From(Guid.NewGuid());
}

  [ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
  public readonly partial record struct CustomerName
  {
    private static Validation Validate(string input) => ValueObjectValidation.ValidateString(input,DomainValidationMessages.CustomerName,minLength: 1,maxLength: 50);

  }

  [ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
  public readonly partial record struct EmailAddress
  {
    private static Validation Validate(string input) => ValueObjectValidation.ValidateString(input, DomainValidationMessages.EmailAddress, minLength: 1, maxLength: 500, customRule: IsValidEmailFormat, customRuleError: DomainValidationMessages.EmailInvalidFormat, customRule2: IsAllowedDomain, customRuleError2: DomainValidationMessages.EmailInvalidDomain);

  private static bool IsValidEmailFormat(string email) => new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").IsMatch(email);
   private static bool IsAllowedDomain(string email)
  {
    var allowedDomains = new[] { EmailDomains.Gmail, EmailDomains.Outlook };
    var emailDomain = email.Split('@').Last();
    return allowedDomains.Contains(emailDomain);
  }
}



