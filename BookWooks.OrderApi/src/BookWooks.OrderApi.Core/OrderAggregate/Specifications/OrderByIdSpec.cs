using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookyWooks.SharedKernel.Specification;

namespace BookWooks.OrderApi.Core.OrderAggregate.Specifications;
public class OrderByIdSpec : BaseSpecification<Order>
{
  public OrderByIdSpec(OrderId id)
       : base(x => x.OrderId == id)
  {
    AddInclude(x => x.OrderItems);
    EnableCache(nameof(OrderByIdSpec), id, TimeSpan.FromMinutes(10));
  }
}
