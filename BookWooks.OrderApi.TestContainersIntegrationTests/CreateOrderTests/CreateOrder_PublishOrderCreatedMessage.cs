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
        using var scope = _apiFactory.Services.CreateScope();
        var testHarness = scope.ServiceProvider.GetRequiredService<ITestHarness>();

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
        var isEventConsumed = await consumerHarness.Consumed.Any<OrderCreatedMessage>(x =>
            x.Context.Message.customerId == message.customerId);

        isEventConsumed.Should().BeTrue();
        await testHarness.Stop();
    }
}
