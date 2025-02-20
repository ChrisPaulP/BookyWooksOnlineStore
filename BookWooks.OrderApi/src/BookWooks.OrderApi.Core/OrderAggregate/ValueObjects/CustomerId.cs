using BookWooks.OrderApi.Core.OrderAggregate.BusinessRuleErrors;


namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct CustomerId
{
  public static Validation Validate(
        Guid customerId)
  {
    var result = Validation.Invalid("Customer Id is missing");

    var isNull = customerId == Guid.Empty;
    if (isNull) result.WithData(BusinessRuleError.Create("Customer Id is required"), string.Empty);
    
    return result.Data is { Count: > 0 }
        ? result
        : Vogen.Validation.Ok;
  }
}
