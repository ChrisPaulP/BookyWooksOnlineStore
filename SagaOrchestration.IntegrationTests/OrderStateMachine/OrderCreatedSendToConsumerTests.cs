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

[Collection("Order Created Send To Consumer Test Collection")]
public class OrderCreatedSendToConsumerTests : SagaTestBase
{

    public OrderCreatedSendToConsumerTests(CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram> factory) 
        : base(factory)
    {

    }

    [Fact]
    public async Task When_OrderCreated_Should_Send_CheckBookStockCommand_ToConsumer()
    {
        await CleanDatabaseAsync();
        // Get fresh harnesses for this test
        var testHarness = GetTestHarness();
        var sagaHarness = GetSagaHarness();
        var consumerHarness = testHarness.GetConsumerHarness<CheckBookStockCommandConsumer>();
        //using var scope = _factory.Services.CreateAsyncScope();
        //var testHarness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        //var sagaHarness = testHarness.GetSagaStateMachineHarness<OrderStateMachine, OrderStateInstance>();
        //var consumerHarness = testHarness.GetConsumerHarness<CheckBookStockCommandConsumer>();
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

            // Assert - Check if the consumer received the command
            var consumed = await consumerHarness.Consumed.Any<CheckBookStockCommand>(x =>
                x.Context.Message.CorrelationId == createdSaga.Saga.CorrelationId);

            Assert.True(consumed, "CheckBookStockCommand should be consumed by the consumer");

            // Verify command details
            var consumedMessage = consumerHarness.Consumed.Select<CheckBookStockCommand>()
                .FirstOrDefault(x => x.Context.Message.CorrelationId == createdSaga.Saga.CorrelationId);

            Assert.NotNull(consumedMessage);
            var command = consumedMessage.Context.Message;
            Assert.Equal(createdSaga.Saga.CorrelationId, command.CorrelationId);
            Assert.Equal(message.orderId, command.orderId);
            Assert.Equal(message.customerId, command.customerId);
        }
        finally
        {
            await testHarness.Stop();
        }
    }
}