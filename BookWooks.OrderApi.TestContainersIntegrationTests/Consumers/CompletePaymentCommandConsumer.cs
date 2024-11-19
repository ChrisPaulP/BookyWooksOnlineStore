namespace BookWooks.OrderApi.TestContainersIntegrationTests.Consumers;

public class CompletePaymentCommandConsumer : IConsumer<CompletePaymentCommand>, ITestConsumer
{
    public async Task Consume(ConsumeContext<CompletePaymentCommand> context)
    {
        // TODO: Send email to customer
        await Task.Delay(1000);
      
    }
}
