using BookyWooks.Messaging.Contracts.Commands;
using BookyWooks.Messaging.Messages.InitialMessage;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestration.IntegrationTests.TestSetup;
using SagaOrchestration.StateInstances;
using SagaOrchestration.StateMachines;

namespace SagaOrchestration.IntegrationTests.OrderStateMachineTests;

[Collection("Order Created Publish To Outbox Test Collection")]
public class OrderCreatedPublishToOutboxTests : SagaTestBase
{
    public OrderCreatedPublishToOutboxTests(CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram> factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task When_OrderCreated_Should_PublishCheckBookStockCommand_ToOutbox()
    {
        await CleanDatabaseAsync();
        //// Get fresh harnesses for this test
        var testHarness = GetTestHarness();
        var sagaHarness = GetSagaHarness();

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

            // Wait for outbox processing using helper
            var outboxMessage = await OutboxTestHelpers.WaitForOutboxMessageAsync(
                Factory.Services, 
                TimeSpan.FromSeconds(10));
                
            Assert.NotNull(outboxMessage);

            // Assert
            var checkBookStockCommand = OutboxTestHelpers.DeserializeOutboxBodyMessage<CheckBookStockCommand>(outboxMessage);
            Assert.NotNull(checkBookStockCommand);

            Assert.NotNull(createdSaga);
            Assert.Equal(createdSaga.Saga.CorrelationId, checkBookStockCommand.CorrelationId);
        }
        finally
        {
            await testHarness.Stop();
        }
    }
}