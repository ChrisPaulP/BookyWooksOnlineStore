namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
[ValueObject<bool>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct OrderPaid
{
  public static readonly OrderPaid True = new(true);
  public static readonly OrderPaid False = new(false);
}
