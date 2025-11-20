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

        var testHarness = GetTestHarness();
        var sagaHarness = GetSagaHarness();

        await testHarness.Start();
        try
        {
            var message = CreateTestOrderMessage();

            await testHarness.Bus.Send(message);
            Assert.True(await sagaHarness.Consumed.Any<OrderCreatedMessage>());

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