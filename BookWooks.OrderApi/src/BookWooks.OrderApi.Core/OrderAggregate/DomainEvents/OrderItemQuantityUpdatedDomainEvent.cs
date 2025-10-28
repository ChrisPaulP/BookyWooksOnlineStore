namespace BookWooks.OrderApi.Core.OrderAggregate.DomainEvents;

using BookyWooks.SharedKernel.Messages;
using System.Text.Json;

public sealed class OrderItemQuantityUpdatedDomainEvent : DomainEventBase, IConvertToOutBoxMessage
{
    public OrderItemId OrderItemId { get; }
    public ProductQuantity NewQuantity { get; }
    public ProductQuantity PreviousQuantity { get; }

    public OrderItemQuantityUpdatedDomainEvent(OrderItemId orderItemId, ProductQuantity newQuantity, ProductQuantity previousQuantity)
    {
        OrderItemId = orderItemId;
        NewQuantity = newQuantity;
        PreviousQuantity = previousQuantity;
    }

    public OutboxMessage ToOutboxMessage()
    {
        return new OutboxMessage(
            Id: Guid.NewGuid(),
            MessageType: nameof(OrderItemQuantityUpdatedDomainEvent),
            Message: JsonSerializer.Serialize(this),
            OccurredOn: DateTimeOffset.UtcNow.DateTime
        );
    }
}
