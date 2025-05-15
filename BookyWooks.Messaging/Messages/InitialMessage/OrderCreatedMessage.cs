using BookyWooks.Messaging.Contracts;
using BookyWooks.Messaging.Contracts.Commands;
using BookyWooks.Messaging.Contracts.Events;

namespace BookyWooks.Messaging.Messages.InitialMessage;

public record OrderCreatedMessage(Guid orderId, Guid customerId, decimal orderTotal, IEnumerable<OrderItemEventDto> orderItems) : MessageContract(nameof(OrderCreatedMessage));

