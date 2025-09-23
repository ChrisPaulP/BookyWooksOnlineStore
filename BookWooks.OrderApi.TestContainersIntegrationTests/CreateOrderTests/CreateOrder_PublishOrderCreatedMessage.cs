namespace BookWooks.OrderApi.TestContainersIntegrationTests.CreateOrderTests;
[Collection("Order Test Collection")]
public class CreateOrder_PublishOrderCreatedMessage
    : ApiTestBase<Program, BookyWooksOrderDbContext>
{
    private readonly TestFactoryBase<Program> _apiFactory;
    public CreateOrder_PublishOrderCreatedMessage(CustomOrderTestFactory<Program> apiFactory)
        : base(apiFactory, () => Task.CompletedTask) => _apiFactory = apiFactory;

    [Fact]
    public async Task PublishOrderCreatedMessage()
    {
        await using var scope = _apiFactory.Services.CreateAsyncScope();
        var testHarness = scope.ServiceProvider.GetRequiredService<ITestHarness>();

        await testHarness.Start(); 

        var orderItems = new List<OrderItemEventDto>
        {
            new OrderItemEventDto(Guid.NewGuid(), 1, true)
        };

        var message = new OrderCreatedMessage(
            orderId: Guid.NewGuid(),
            customerId: Guid.NewGuid(),
            orderTotal: 9.99M,
            orderItems: orderItems
        );

        await testHarness.Bus.Publish(message);

        var consumerHarness = testHarness.GetConsumerHarness<OrderCreatedConsumer>();

        var isEventConsumed = false;
        for (var retry = 0; retry < 10 && !isEventConsumed; retry++)
        {
            isEventConsumed = await consumerHarness.Consumed
                .Any<OrderCreatedMessage>(x => x.Context.Message.customerId == message.customerId);

            if (!isEventConsumed)
                await Task.Delay(500);
        }
        isEventConsumed.Should().BeTrue("the OrderCreatedMessage should be consumed");
    }
}
