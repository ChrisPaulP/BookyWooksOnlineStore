
using MassTransit;

namespace BookyWooks.Messaging.Contracts.Events;

public record PaymentRequestedEvent(Guid CorrelationId, Guid customerId, decimal orderTotal) : MessageContract, CorrelatedBy<Guid>;

