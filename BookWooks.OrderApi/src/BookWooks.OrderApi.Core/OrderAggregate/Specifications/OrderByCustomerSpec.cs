using BookWooks.OrderApi.Core.OrderAggregate.Entities;

namespace BookWooks.OrderApi.Core.OrderAggregate.Specifications;
public class OrderByCustomerSpec : BaseSpecification<Order>
{
  public OrderByCustomerSpec(Guid customerId)
       : base(x => x.CustomerId == customerId)
  {
    AddInclude(x => x.OrderItems);
    AddInclude("");
  }
}
