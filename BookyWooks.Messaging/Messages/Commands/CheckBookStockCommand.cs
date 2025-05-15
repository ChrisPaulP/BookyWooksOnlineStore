using BookyWooks.Messaging.Contracts.Events;

using MassTransit;


namespace BookyWooks.Messaging.Contracts.Commands;

public record CheckBookStockCommand(Guid CorrelationId, Guid orderId, Guid customerId, decimal orderTotal, IEnumerable<OrderItemEventDto> orderItems) : MessageContract(""), CorrelatedBy<Guid>;

