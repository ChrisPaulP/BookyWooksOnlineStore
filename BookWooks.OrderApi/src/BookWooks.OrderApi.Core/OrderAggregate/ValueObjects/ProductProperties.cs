
using BookWooks.OrderApi.Core.OrderAggregate.BusinessRuleErrors;

namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;


[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public sealed partial record ProductId
{
    public static Validation Validate( Guid productId)
    {
      var result = Validation.Invalid("Product Id is missing");

      var isNull = productId == Guid.Empty;
      if (isNull) result.WithData(BusinessRuleError.Create("Product Id is required"), string.Empty);

      return result.Data is { Count: > 0 }
          ? result
          : Vogen.Validation.Ok;
    }
  public static ProductId New()
  {
    return From(Guid.NewGuid());
  }
  //public bool Equals(ProductId other) // ⚠ No "override" needed in record struct!
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
  public readonly partial record struct ProductName
  {
    private static Validation Validate(string input)
    {
      var result = Validation.Invalid("ProductName does not meet requirements");
      if (string.IsNullOrWhiteSpace(input)) result.WithData(BusinessRuleError.Create("ProductName is required"), string.Empty);
      if (input.Length > 100) result.WithData(BusinessRuleError.Create("ProductName must be less than 100 characters"), string.Empty);
      return result.Data is { Count: > 0 } ? result : Validation.Ok;
    }
  }

  [ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
  public readonly partial record struct ProductDescription
  {
    private static Validation Validate(string input)
    {
      var result = Validation.Invalid("ProductDescription does not meet requirements");
      if (string.IsNullOrWhiteSpace(input)) result.WithData(BusinessRuleError.Create("ProductDescription is required"), string.Empty);
      if (input.Length > 500) result.WithData(BusinessRuleError.Create("ProductDescription must be less than 500 characters"), string.Empty);
      return result.Data is { Count: > 0 } ? result : Validation.Ok;
    }
  }
[ValueObject<decimal>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial record struct ProductPrice
{
  private static Validation Validate(decimal input)
  {
    var result = Validation.Invalid("Price does not meet requirements");
    if (input <= 0) result.WithData(BusinessRuleError.Create("Price must be greater than zero"), string.Empty);
    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }
}
[ValueObject<int>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial record struct ProductQuantity
{
  private static Validation Validate(int input)
  {
    var result = Validation.Invalid("Quantity does not meet requirements");
    if (input <= 0) result.WithData(BusinessRuleError.Create("Quantity must be greater than zero"), string.Empty);
    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }
}

