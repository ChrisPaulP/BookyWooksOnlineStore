namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public sealed partial record OrderItemId
{
    public static Validation Validate( Guid orderItemId)
    {
      var result = Validation.Invalid("Product Id is missing");

      var isNull = orderItemId == Guid.Empty;
      if (isNull) result.WithData(BusinessRuleError.Create("Order Item Id is required"), string.Empty);

      return result.Data is { Count: > 0 } ? result: Vogen.Validation.Ok;
    }
  public static OrderItemId New()
  {
    return From(Guid.NewGuid());
  }
}

 




