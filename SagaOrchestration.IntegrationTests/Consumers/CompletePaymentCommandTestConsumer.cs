using BookyWooks.Messaging.Contracts.Commands;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaOrchestration.IntegrationTests.Consumers;

public class CompletePaymentCommandTestConsumer : IConsumer<CompletePaymentCommand>
{
    public async Task Consume(ConsumeContext<CompletePaymentCommand> context)
    {
        Console.WriteLine($"Test consumer received CompletePaymentCommand for CorrelationId: {context.Message.CorrelationId}");

        // For testing purposes, you can simulate success or failure
        // You might want to publish a PaymentCompletedEvent if your saga handles it

        await Task.CompletedTask;
    }
}
