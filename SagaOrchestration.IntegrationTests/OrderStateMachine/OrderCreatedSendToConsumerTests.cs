namespace SagaOrchestration.IntegrationTests.OrderStateMachineTests;

[Collection("Order Created Send To Consumer Test Collection")]
public class OrderCreatedSendToConsumerTests : SagaTestBase
{
    public OrderCreatedSendToConsumerTests(CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram> factory) 
        : base(factory)
    {}

    [Fact]
    public async Task When_OrderCreated_Should_Send_CheckBookStockCommand_ToConsumer()
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

            var consumed = await consumerHarness.Consumed.Any<CheckBookStockCommand>(x =>
                x.Context.Message.CorrelationId == createdSaga.Saga.CorrelationId);

            Assert.True(consumed, "CheckBookStockCommand should be consumed by the consumer");

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