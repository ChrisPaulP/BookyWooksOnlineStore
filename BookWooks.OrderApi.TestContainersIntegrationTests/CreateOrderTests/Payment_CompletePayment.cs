namespace BookWooks.OrderApi.TestContainersIntegrationTests.CreateOrderTests;
[Collection("Order Test Collection")]
public class Payment_CompletePayment : ApiTestBase<Program, BookyWooksOrderDbContext>
{
    private readonly CustomOrderTestFactory<Program> _apiFactory;

    public Payment_CompletePayment(CustomOrderTestFactory<Program> apiFactory)
        : base(apiFactory, () => Task.CompletedTask)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task CompletePayment()
    {
        using var scope = _apiFactory.Services.CreateAsyncScope();
        var testHarness = scope.ServiceProvider.GetRequiredService<ITestHarness>();

        await testHarness.Start();

        var command = new CompletePaymentCommand(
            CorrelationId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            OrderTotal: 9.99M
        );

        await testHarness.Bus.Publish(command);

        var isEventPublished = await testHarness.Published.Any<CompletePaymentCommand>();
        isEventPublished.Should().BeTrue("the payment command should be published to the bus");

        var consumerHarness = testHarness.GetConsumerHarness<CompletePaymentCommandConsumer>();
        var isEventConsumed = false;
        for (var retry = 0; retry < 10 && !isEventConsumed; retry++)
        {
            isEventConsumed = await consumerHarness.Consumed.Any<CompletePaymentCommand>(
            x => x.Context.Message.CustomerId == command.CustomerId);

            if (!isEventConsumed)
                await Task.Delay(500);
        }
        isEventConsumed.Should().BeTrue("the consumer should process the published payment command");
    }
}


