namespace BookWooks.OrderApi.Core.OrderAggregate.Specifications;
public class OrderByIdSpec : BaseSpecification<Order>
{
  public OrderByIdSpec(OrderId id): base(x => x.OrderId == id)
  {
    AddInclude(Order.OrderItemsFieldName);
    EnableCache(nameof(OrderByIdSpec), id, TimeSpan.FromMinutes(10));
  }
}
