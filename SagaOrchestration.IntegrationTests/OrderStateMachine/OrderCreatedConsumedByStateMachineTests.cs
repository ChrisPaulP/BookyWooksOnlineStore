using BookWooks.OrderApi.Infrastructure.Data;
using BookyWooks.Messaging.Messages.InitialMessage;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestration.IntegrationTests.TestSetup;
using SagaOrchestration.StateInstances;
using SagaOrchestration.StateMachines;

namespace SagaOrchestration.IntegrationTests.OrderStateMachineTests;

[Collection("Order Created Consumed Test Collection")]
public class OrderCreatedConsumedByStateMachineTests : SagaTestBase
{
    

    public OrderCreatedConsumedByStateMachineTests(CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram> factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task When_OrderCreated_Should_BeConsumedByStateMachine()
    {
        // Arrange test harnesses from SagaTestBase (correct)
        var testHarness = GetTestHarness();
        var sagaHarness = GetSagaHarness();

        await testHarness.Start();
        try
        {
            var message = CreateTestOrderMessage();

            // Act
            await testHarness.Bus.Send(message);

            // Assert saga consumed
            Assert.True(await sagaHarness.Consumed.Any<OrderCreatedMessage>());

            // Assert inbox used
            var inboxState = await FindAllAsync<InboxState>();
            Assert.True(inboxState.Any(), "InboxState should contain the consumed message");
        }
        finally
        {
            await testHarness.Stop();
        }
    }
}
