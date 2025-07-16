namespace BookWooks.OrderApi.Core.OrderAggregate.Specifications;
public class OrderByCustomerIdSpec : BaseSpecification<Order>
{
  public OrderByCustomerIdSpec(CustomerId customerId): base(x => x.CustomerId == customerId)
  {
    AddInclude(Order.OrderItemsFieldName);
    EnableCache(nameof(OrderByCustomerIdSpec), customerId, TimeSpan.FromMinutes(10));
  }
}
