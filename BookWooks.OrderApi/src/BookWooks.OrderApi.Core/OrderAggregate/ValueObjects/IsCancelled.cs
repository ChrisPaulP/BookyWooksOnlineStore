namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
[ValueObject<bool>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct IsCancelled
{
  public static readonly IsCancelled True = new(true);
  public static readonly IsCancelled False = new(false);
}
