namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct Message
{
  public static Validation Validate(
        Message message)
  {
    var result = Validation.Invalid("A message is missing");

    var isNull = message == string.Empty;
    if (isNull) result.WithData("A message is required", string.Empty);

    return result.Data is { Count: > 0 }
        ? result
        : Vogen.Validation.Ok;
  }
}
