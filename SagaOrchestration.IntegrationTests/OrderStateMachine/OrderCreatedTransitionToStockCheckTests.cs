using BookWooks.OrderApi.TestContainersIntegrationTests.TestSetup;
using BookyWooks.Messaging.Messages.InitialMessage;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestration.IntegrationTests.TestSetup;
using SagaOrchestration.StateInstances;
using SagaOrchestration.StateMachines;

namespace SagaOrchestration.IntegrationTests.OrderStateMachineTests;

[Collection("Order Created Transition To Stock Check Test Collection")]
public class OrderCreatedTransitionToStockCheckTests : SagaTestBase
{

    public OrderCreatedTransitionToStockCheckTests(CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram> factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task When_OrderCreated_Should_TransitionToStockCheckState()
    {
         await CleanDatabaseAsync();

        //using var scope = _factory.Services.CreateAsyncScope();
        //var testHarness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        //var sagaHarness = testHarness.GetSagaStateMachineHarness<OrderStateMachine, OrderStateInstance>();

        // Get fresh harnesses for this test
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

            // Assert
            var createdSaga = sagaHarness.Created
                .Select(x => x.Saga.OrderId == message.orderId)
                .FirstOrDefault();
            
            Assert.NotNull(createdSaga);

            var instance = sagaHarness.Created.ContainsInState(
                createdSaga.Saga.CorrelationId,
                sagaHarness.StateMachine,
                sagaHarness.StateMachine.StockCheck);

            Assert.NotNull(instance);
        }
        finally
        {
            await testHarness.Stop();
        }
    }
}