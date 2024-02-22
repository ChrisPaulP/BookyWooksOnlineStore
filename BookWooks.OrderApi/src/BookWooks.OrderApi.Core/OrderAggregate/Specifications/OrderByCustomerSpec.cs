namespace BookWooks.OrderApi.Core.OrderAggregate.Specifications;
public class OrderByCustomerSpec : BaseSpecification<Order>
{
  public OrderByCustomerSpec(int customerId)
       : base(x => x.CustomerId == customerId)
  {
    AddInclude(x => x.OrderItems);
    AddInclude("");
  }
}
