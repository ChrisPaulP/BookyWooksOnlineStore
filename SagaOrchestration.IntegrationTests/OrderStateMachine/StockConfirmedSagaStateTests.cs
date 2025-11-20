namespace SagaOrchestration.IntegrationTests.OrderStateMachineTests;

[Collection("Stock Confirmed Saga State Test Collection")]
public class StockConfirmedSagaStateTests : SagaTestBase
{
    public StockConfirmedSagaStateTests(CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram> factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task When_StockConfirmed_Should_UpdateSagaState_And_CreateInboxEntry()
    {
        await CleanDatabaseAsync();

        var testHarness = GetTestHarness();
        var sagaHarness = GetSagaHarness();
        var consumerHarness = testHarness.GetConsumerHarness<CheckBookStockCommandConsumer>();

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

            var stockItems = new List<OrderItemEventDto>();

            var stockConfirmedEvent = new StockConfirmedEvent(
                createdSaga.Saga.CorrelationId,
                message.orderId,
                stockItems);
            await testHarness.Bus.Publish(stockConfirmedEvent);

            Assert.True(await sagaHarness.Consumed.Any<StockConfirmedEvent>());

            var sagaInstance = sagaHarness.Sagas
            .Select(x => x.CorrelationId == createdSaga.Saga.CorrelationId)
            .FirstOrDefault();

            Assert.NotNull(sagaInstance);
            var inboxState = await FindAllAsync<InboxState>();
            Assert.Equal(2, inboxState.Count);

            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<StateMachineDbContext>();
            var dbSaga = await dbContext.Set<OrderStateInstance>()
            .Where(x => x.CorrelationId == createdSaga.Saga.CorrelationId)
            .FirstOrDefaultAsync();

            Assert.NotNull(dbSaga);
            Assert.Equal(sagaHarness.StateMachine.StockReserved.Name, dbSaga.CurrentState);
        }
        finally
        {
            await testHarness.Stop();
        }
    }
}