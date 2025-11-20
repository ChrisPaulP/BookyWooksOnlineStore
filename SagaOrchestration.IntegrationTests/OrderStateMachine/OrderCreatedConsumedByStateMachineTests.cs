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
        var testHarness = GetTestHarness();
        var sagaHarness = GetSagaHarness();

        await testHarness.Start();
        try
        {
            var message = CreateTestOrderMessage();

            await testHarness.Bus.Send(message);

            Assert.True(await sagaHarness.Consumed.Any<OrderCreatedMessage>());

            var inboxState = await FindAllAsync<InboxState>();
            Assert.True(inboxState.Any(), "InboxState should contain the consumed message");
        }
        finally
        {
            await testHarness.Stop();
        }
    }
}
