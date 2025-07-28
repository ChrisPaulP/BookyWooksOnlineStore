using BookWooks.OrderApi.TestContainersIntegrationTests;
using FluentAssertions.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BookWooks.OrderApi.TestContainersIntegrationTests.CreateOrderTests;
[Collection("Order Test Collection")]
public class Payment_CompletePayment : ApiTestBase<Program, BookyWooksOrderDbContext>
{
    private readonly TestFactoryBase<Program> _apiFactory;
    public Payment_CompletePayment(CustomOrderTestFactory<Program> apiFactory)
            : base(apiFactory, apiFactory.DisposeAsync)
        {
            _apiFactory = apiFactory;
    }

        [Fact]
        public async Task CompletePayment()
        {
            using var scope = _apiFactory.Services.CreateScope();
            var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();

            await harness.Start();

            var command = new CompletePaymentCommand(
                CorrelationId: Guid.NewGuid(),
                CustomerId: Guid.NewGuid(),
                OrderTotal: 9.99M
            );

            await harness.Bus.Publish(command);

            var isEventPublished = await harness.Published.Any<CompletePaymentCommand>();
            isEventPublished.Should().BeTrue("the payment command should be published to the bus");

            var consumerHarness = harness.GetConsumerHarness<CompletePaymentCommandConsumer>();
            var isEventConsumed = await consumerHarness.Consumed.Any<CompletePaymentCommand>(
                x => x.Context.Message.CustomerId == command.CustomerId);

            isEventConsumed.Should().BeTrue("the consumer should process the published payment command");

            await harness.Stop();
        }

        [Fact]
        public async Task CompletePayment2()
        {
        using var scope = _apiFactory.Services.CreateScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();

        await harness.Start();

            var endPointName = harness.EndpointNameFormatter.Consumer<CompletePaymentCommandConsumer>();

            var command = new CompletePaymentCommand(
                CorrelationId: Guid.NewGuid(),
                CustomerId: Guid.NewGuid(),
                OrderTotal: 9.99M
            );

            await harness.Bus.Publish(command);

            var isEventPublished = await harness.Published.Any<CompletePaymentCommand>();
            isEventPublished.Should().BeTrue("the payment command should be published to the bus");

            var consumerHarness = harness.GetConsumerHarness<CompletePaymentCommandConsumer>();
            var isEventConsumed = await consumerHarness.Consumed.Any<CompletePaymentCommand>(
                x => x.Context.Message.CustomerId == command.CustomerId);

            isEventConsumed.Should().BeTrue("the consumer should process the published payment command");

            await harness.Stop();
        }
    }


