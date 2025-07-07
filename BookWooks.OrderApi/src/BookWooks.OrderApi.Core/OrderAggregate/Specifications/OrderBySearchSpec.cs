namespace BookWooks.OrderApi.Core.OrderAggregate.Specifications;
public class OrderBySearchSpec : BaseSpecification<Order>
{
  public OrderBySearchSpec(OrderId orderId): base(x => orderId == Guid.Empty ? true : x.OrderId == orderId)
  {
    AddInclude(Order.OrderItemsFieldName);
  }
}
