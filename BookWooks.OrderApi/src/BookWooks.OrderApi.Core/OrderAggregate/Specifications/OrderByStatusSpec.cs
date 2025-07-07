namespace BookWooks.OrderApi.Core.OrderAggregate.Specifications;
public class OrderByStatusSpec : BaseSpecification<Order>
{
  public OrderByStatusSpec(string status): base(x => x.Status.Label == status)
  {
    AddInclude(Order.OrderItemsFieldName);
    EnableCache(nameof(OrderByStatusSpec), status, TimeSpan.FromMinutes(10));
  }
}

