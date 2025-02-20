namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
[ValueObject<DateTimeOffset>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct OrderPlaced
{
}
