namespace SagaOrchestration.IntegrationTests.Consumers;

public class CompletePaymentCommandTestConsumer : IConsumer<CompletePaymentCommand>
{
    public async Task Consume(ConsumeContext<CompletePaymentCommand> context)
    {
        Console.WriteLine($"Test consumer received CompletePaymentCommand for CorrelationId: {context.Message.CorrelationId}");

        await Task.CompletedTask;
    }
}
