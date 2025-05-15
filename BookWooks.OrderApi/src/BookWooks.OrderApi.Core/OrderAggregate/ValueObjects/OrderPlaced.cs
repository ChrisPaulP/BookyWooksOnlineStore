namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
[ValueObject<DateTimeOffset>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct OrderPlaced
{
  //private static Validation Validate(DateTimeOffset input)
  //{
  //  var result = Validation.Invalid("A message is missing");

  //  var isNull = input < DateTimeOffset.UtcNow;
  //  if (isNull) result.WithData("A message is required", string.Empty);

  //  return result.Data is { Count: > 0 } ? result : Vogen.Validation.Ok;
  //}
}
