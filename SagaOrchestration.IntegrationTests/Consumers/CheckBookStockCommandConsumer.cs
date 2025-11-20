namespace SagaOrchestration.IntegrationTests.Consumers;

public class CheckBookStockCommandConsumer : IConsumer<CheckBookStockCommand>, ITestConsumer
{
    public async Task Consume(ConsumeContext<CheckBookStockCommand> context)
    {
        await Task.Delay(1000);

    }
}
