namespace BookWooks.OrderApi.TestContainersIntegrationTests.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedMessage>, ITestConsumer
{
    public async Task Consume(ConsumeContext<OrderCreatedMessage> context)
    {
        // TODO: Send email to customer
        await Task.Delay(1000);

    }
}
