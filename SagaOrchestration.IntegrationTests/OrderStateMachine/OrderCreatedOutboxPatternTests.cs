using BookyWooks.Messaging.Contracts.Commands;
using BookyWooks.Messaging.Messages.InitialMessage;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestration.IntegrationTests.Consumers;
using SagaOrchestration.IntegrationTests.TestSetup;
using SagaOrchestration.StateInstances;
using SagaOrchestration.StateMachines;

namespace SagaOrchestration.IntegrationTests.OrderStateMachineTests;

[Collection("Order Created Outbox Test Collection")]
public class OrderCreatedOutboxPatternTests : SagaTestBase
{
    public OrderCreatedOutboxPatternTests(CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram> factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task When_OrderCreated_Should_UseOutboxPattern_ForReliableMessaging()
    {
        await CleanDatabaseAsync();
        
        // Get fresh harnesses for this test
        var testHarness = GetTestHarness();
        var sagaHarness = GetSagaHarness();
        var consumerHarness = testHarness.GetConsumerHarness<CheckBookStockCommandConsumer>();

        await testHarness.Start();
        try
        {
            // Arrange
            var message = CreateTestOrderMessage();

            // Act
            await testHarness.Bus.Send(message);
            Assert.True(await sagaHarness.Consumed.Any<OrderCreatedMessage>());

            var createdSaga = sagaHarness.Created
                .Select(x => x.Saga.OrderId == message.orderId)
                .FirstOrDefault();
            Assert.NotNull(createdSaga);

            // Wait a bit for the saga to process and write to outbox
            //await Task.Delay(500);

            // Assert - Verify outbox was used
            var outboxMessage = await OutboxTestHelpers.WaitForOutboxMessageAsync(
                Factory.Services, 
                TimeSpan.FromSeconds(10));
                
            Assert.NotNull(outboxMessage);

            // **CRITICAL**: MassTransit's outbox delivery service runs on a background timer
            // The QueryDelay is set to 100ms in your config, but delivery can take multiple cycles
            // Wait significantly longer to allow for:
            // 1. Outbox lock acquisition
            // 2. Message serialization
            // 3. Delivery to RabbitMQ
            // 4. Consumer processing
            //await Task.Delay(5000); // Wait 5 seconds for outbox delivery cycles

            // Verify the command was delivered to the consumer
            var consumed = await consumerHarness.Consumed.Any<CheckBookStockCommand>(x =>
                x.Context.Message.CorrelationId == createdSaga.Saga.CorrelationId);

            Assert.True(consumed, "Message should be delivered to consumer through outbox");
        }
        finally
        {
            await testHarness.Stop();
        }
    }
}