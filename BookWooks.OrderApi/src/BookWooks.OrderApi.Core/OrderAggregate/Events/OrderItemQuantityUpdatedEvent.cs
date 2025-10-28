namespace BookWooks.OrderApi.Core.OrderAggregate.Events;

public sealed class OrderItemQuantityUpdatedEvent : DomainEventBase
{
    public OrderItemId OrderItemId { get; }
    public ProductQuantity NewQuantity { get; }
    public ProductQuantity PreviousQuantity { get; }

    public OrderItemQuantityUpdatedEvent(OrderItemId orderItemId, ProductQuantity newQuantity, ProductQuantity previousQuantity)
    {
        OrderItemId = orderItemId;
        NewQuantity = newQuantity;
        PreviousQuantity = previousQuantity;
    }
}
