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

            var outboxMessage = await OutboxTestHelpers.WaitForOutboxMessageAsync(
                Factory.Services, 
                TimeSpan.FromSeconds(10));
                
            Assert.NotNull(outboxMessage);

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