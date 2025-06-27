namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public sealed partial record OrderItemId
{
  public static Validation Validate( Guid orderItemId) => ValueObjectValidation.Validate(orderItemId, ValidationMessages.OrderItemId);
  public static OrderItemId New() => From(Guid.NewGuid());
}

 




