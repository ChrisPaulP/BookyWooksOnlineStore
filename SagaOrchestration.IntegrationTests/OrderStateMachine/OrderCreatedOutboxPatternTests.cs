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

            var outboxMessage = await OutboxTestHelpers.WaitForOutboxMessageAsync(
                Factory.Services, 
                TimeSpan.FromSeconds(10));
                
            Assert.NotNull(outboxMessage);

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