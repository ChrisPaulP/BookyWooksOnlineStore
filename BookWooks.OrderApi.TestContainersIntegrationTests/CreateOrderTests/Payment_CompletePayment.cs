using Microsoft.Extensions.DependencyInjection;

namespace BookWooks.OrderApi.TestContainersIntegrationTests.CreateOrderTests;
[Collection("Order Test Collection")]
public class Payment_CompletePayment : ApiTestBase<Program, BookyWooksOrderDbContext>
{
    private readonly TestFactoryBase<Program> _apiFactory;
    public Payment_CompletePayment(CustomOrderTestFactory<Program> apiFactory)
            : base(apiFactory, apiFactory.DisposeAsync) => _apiFactory = apiFactory;

        [Fact]
        public async Task CompletePayment()
        {
        var _testHarness = _apiFactory.Services.GetRequiredService<ITestHarness>();
        await _testHarness.Start();

            var command = new CompletePaymentCommand(
                CorrelationId: Guid.NewGuid(),
                CustomerId: Guid.NewGuid(),
                OrderTotal: 9.99M
            );

            await _testHarness.Bus.Publish(command);

            var isEventPublished = await _testHarness.Published.Any<CompletePaymentCommand>();
            isEventPublished.Should().BeTrue("the payment command should be published to the bus");

            var consumerHarness = _testHarness.GetConsumerHarness<CompletePaymentCommandConsumer>();
            var isEventConsumed = await consumerHarness.Consumed.Any<CompletePaymentCommand>(
                x => x.Context.Message.CustomerId == command.CustomerId);

            isEventConsumed.Should().BeTrue("the consumer should process the published payment command");

            await _testHarness.Stop();
        }

        [Fact]
        public async Task CompletePayment2()
        {
        var _testHarness = _apiFactory.Services.GetRequiredService<ITestHarness>();
        await _testHarness.Start();

            var endPointName = _testHarness.EndpointNameFormatter.Consumer<CompletePaymentCommandConsumer>();

            var command = new CompletePaymentCommand(
                CorrelationId: Guid.NewGuid(),
                CustomerId: Guid.NewGuid(),
                OrderTotal: 9.99M
            );

            await _testHarness.Bus.Publish(command);

            var isEventPublished = await _testHarness.Published.Any<CompletePaymentCommand>();
            isEventPublished.Should().BeTrue("the payment command should be published to the bus");

            var consumerHarness = _testHarness.GetConsumerHarness<CompletePaymentCommandConsumer>();
            var isEventConsumed = await consumerHarness.Consumed.Any<CompletePaymentCommand>(
                x => x.Context.Message.CustomerId == command.CustomerId);

            isEventConsumed.Should().BeTrue("the consumer should process the published payment command");

            await _testHarness.Stop();
        }
    }


