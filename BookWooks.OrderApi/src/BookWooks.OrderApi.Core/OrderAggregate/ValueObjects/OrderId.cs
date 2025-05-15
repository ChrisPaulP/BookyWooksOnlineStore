

namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;


[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
//public readonly partial record struct OrderId
public sealed partial record OrderId
{
  public static Validation Validate(Guid input)
  {
    var result = Validation.Invalid("Order Id is missing");

    var isNull = input == Guid.Empty;
    if (isNull) result.WithData(BusinessRuleError.Create("Order Id is required"), string.Empty);

    return result.Data is { Count: > 0 }
        ? result
        : Vogen.Validation.Ok;
  }

  public static OrderId New()
  {
    return From(Guid.NewGuid());
  }
  //private OrderId() : this(Guid.Empty) { }
  //public bool Equals(OrderId other) // ⚠ No "override" needed in record struct!
  //{
  //  Console.WriteLine($"🔍 Comparing OrderId: {Value} with {other.Value}");

  //  if (!IsInitialized() || !other.IsInitialized())
  //  {
  //    Console.WriteLine("❌ One of the OrderId instances is not initialized!");
  //    return false;
  //  }

  //  return Value.Equals(other.Value);
  //}

  //public override int GetHashCode()
  //{
  //  if (!IsInitialized())
  //  {
  //    Console.WriteLine("❌ Attempting to get hash code of an uninitialized OrderId!");
  //    throw new InvalidOperationException("OrderId is not initialized.");
  //  }

  //  return Value.GetHashCode();
  //}
}

