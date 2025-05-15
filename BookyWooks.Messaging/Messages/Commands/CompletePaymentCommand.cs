using MassTransit;
namespace BookyWooks.Messaging.Contracts.Commands;

public record CompletePaymentCommand(Guid CorrelationId, Guid CustomerId, decimal OrderTotal): MessageContract(nameof(CompletePaymentCommand)), CorrelatedBy<Guid>;

