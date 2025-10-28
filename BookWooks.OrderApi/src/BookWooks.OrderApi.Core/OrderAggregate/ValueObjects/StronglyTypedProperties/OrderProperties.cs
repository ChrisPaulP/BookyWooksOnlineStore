namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
//public readonly partial record struct OrderId
public sealed partial record OrderId
{
  public static Validation Validate(Guid input)  => ValueObjectValidation.ValidateGuid(input, DomainValidationMessages.OrderId);
  public static OrderId New() => From(Guid.NewGuid());
}

  [ValueObject<bool>(conversions: Conversions.EfCoreValueConverter)]
  public partial record struct OrderPaid
  {
    public static readonly OrderPaid True = new(true);
    public static readonly OrderPaid False = new(false);
  }
  [ValueObject<DateTimeOffset>(conversions: Conversions.EfCoreValueConverter)]
  public partial record struct OrderPlaced
  {
  private static Validation Validate(DateTimeOffset input) => ValueObjectValidation.ValidateDateTime(input, DomainValidationMessages.DateTime);
}
  [ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
  public partial record struct Message
  {
    public static Validation Validate(string message) => ValueObjectValidation.ValidateString(message, DomainValidationMessages.OrderMessage, minLength: 1, maxLength: 50);
  }
[ValueObject<bool>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct IsCancelled
{
  public static readonly IsCancelled True = new(true);
  public static readonly IsCancelled False = new(false);
}
