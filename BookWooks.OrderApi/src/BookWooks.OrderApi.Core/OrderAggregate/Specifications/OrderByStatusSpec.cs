using BookWooks.OrderApi.Core.OrderAggregate.Entities;

namespace BookWooks.OrderApi.Core.OrderAggregate.Specifications;
public class OrderByStatusSpec : BaseSpecification<Order>
{
  public OrderByStatusSpec(string status)
       : base(x => x.Status.Label == status)
  {
    AddInclude(x => x.OrderItems);
    EnableCache(nameof(OrderByStatusSpec), status, TimeSpan.FromMinutes(10));
  }
}

