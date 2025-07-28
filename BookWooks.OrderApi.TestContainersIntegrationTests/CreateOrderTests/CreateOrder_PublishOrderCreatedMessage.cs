using Microsoft.Extensions.DependencyInjection;

namespace BookWooks.OrderApi.TestContainersIntegrationTests.CreateOrderTests;
[Collection("Order Test Collection")]
public class CreateOrder_PublishOrderCreatedMessage
    : ApiTestBase<Program, BookyWooksOrderDbContext>
{
    private readonly TestFactoryBase<Program> _apiFactory;
    public CreateOrder_PublishOrderCreatedMessage(CustomOrderTestFactory<Program> apiFactory)
        : base(apiFactory, apiFactory.DisposeAsync) => _apiFactory = apiFactory;

    [Fact]
    public async Task PublishOrderCreatedMessage()
    {
        var _testHarness = _apiFactory.Services.GetRequiredService<ITestHarness>();
        await _testHarness.Start();

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

        await _testHarness.Bus.Publish(message);

        var consumerHarness = _testHarness.GetConsumerHarness<OrderCreatedConsumer>();
        var isEventConsumed = await consumerHarness.Consumed.Any<OrderCreatedMessage>(x =>
            x.Context.Message.customerId == message.customerId);

        isEventConsumed.Should().BeTrue();
        await _testHarness.Stop();
    }
}
