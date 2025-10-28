namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public sealed partial record ProductId
{
  public static Validation Validate(Guid productId) =>
      ValueObjectValidation.ValidateGuid(productId, DomainValidationMessages.ProductId);

  public static ProductId New() => From(Guid.NewGuid());
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial record struct ProductName
{
  private static Validation Validate(string input) =>
      ValueObjectValidation.ValidateString(input, DomainValidationMessages.ProductName, minLength: 1, maxLength: 100);
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial record struct ProductDescription
{
  private static Validation Validate(string input) =>
      ValueObjectValidation.ValidateString(input, DomainValidationMessages.ProductDescription, minLength: 1, maxLength: 500);
}

[ValueObject<decimal>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial record struct ProductPrice
{
  private static Validation Validate(decimal input) =>
      ValueObjectValidation.ValidateDecimal(input, DomainValidationMessages.ProductPrice, minValue: 0);
}

[ValueObject<int>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial record struct ProductQuantity
{
  private static Validation Validate(int input) =>
      ValueObjectValidation.ValidateInt(input, DomainValidationMessages.ProductQuantity, minValue: 0);
}
