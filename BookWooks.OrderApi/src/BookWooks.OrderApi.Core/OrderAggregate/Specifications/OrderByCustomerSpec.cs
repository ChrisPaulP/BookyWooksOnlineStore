namespace BookWooks.OrderApi.Core.OrderAggregate.Specifications;
public class OrderByCustomerSpec : BaseSpecification<Order>
{
  public OrderByCustomerSpec(CustomerId customerId): base(x => x.CustomerId == customerId) => AddInclude(Order.OrderItemsFieldName);
}
