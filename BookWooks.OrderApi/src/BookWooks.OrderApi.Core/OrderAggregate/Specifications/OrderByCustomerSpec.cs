using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookyWooks.SharedKernel.Specification;

namespace BookWooks.OrderApi.Core.OrderAggregate.Specifications;
public class OrderByCustomerSpec : BaseSpecification<Order>
{
  public OrderByCustomerSpec(CustomerId customerId)
       : base(x => x.CustomerId == customerId)
  {
    AddInclude(x => x.OrderItems);
    AddInclude("");
  }
}
