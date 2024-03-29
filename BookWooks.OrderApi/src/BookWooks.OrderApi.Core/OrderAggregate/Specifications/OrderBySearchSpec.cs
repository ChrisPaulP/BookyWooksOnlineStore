using BookWooks.OrderApi.Core.OrderAggregate.Entities;

namespace BookWooks.OrderApi.Core.OrderAggregate.Specifications;
public class OrderBySearchSpec : BaseSpecification<Order>
{
  public OrderBySearchSpec(Guid orderId = default)
      : base(x => orderId == Guid.Empty ? true : x.Id == orderId)
  {
    AddInclude(x => x.OrderItems);
  }
}
