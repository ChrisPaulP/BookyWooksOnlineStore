using BookWooks.OrderApi.Infrastructure.Data;
using BookyWooks.Messaging.Contracts.Events;
using BookyWooks.Messaging.Messages.InitialMessage;
using FluentAssertions.Common;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestration.Data;
using SagaOrchestration.IntegrationTests.Consumers;
using SagaOrchestration.IntegrationTests.TestSetup;
using SagaOrchestration.StateInstances;
using SagaOrchestration.StateMachines;

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
        // Clean database before test to ensure isolation
        await CleanDatabaseAsync();

        //// Get fresh harnesses for this test
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
            // Arrange - Create initial order
            var message = CreateTestOrderMessage();
            await testHarness.Bus.Send(message);
            Assert.True(await sagaHarness.Consumed.Any<OrderCreatedMessage>());

            var createdSaga = sagaHarness.Created
                .Select(x => x.Saga.OrderId == message.orderId)
                .FirstOrDefault();
            Assert.NotNull(createdSaga);

            var stockItems = new List<OrderItemEventDto>();

            // Act - Send stock confirmed event
            var stockConfirmedEvent = new StockConfirmedEvent(
                createdSaga.Saga.CorrelationId,
                message.orderId,
                stockItems);
            await testHarness.Bus.Publish(stockConfirmedEvent);

            // Assert
            Assert.True(await sagaHarness.Consumed.Any<StockConfirmedEvent>());

            // Verify inbox state updated
            // Expected entries: OrderCreated (1) + CheckBookStockCommand outbox (1) + StockConfirmed (1) = 3
            var sagaInstance = sagaHarness.Sagas
            .Select(x => x.CorrelationId == createdSaga.Saga.CorrelationId)
            .FirstOrDefault();

            Assert.NotNull(sagaInstance);
            //Assert.Equal(sagaHarness.StateMachine.StockReserved.Name, sagaInstance.CurrentState);
            var inboxState = await FindAllAsync<InboxState>();
            Assert.Equal(2, inboxState.Count);


            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<StateMachineDbContext>();
            var dbSaga = await dbContext.Set<OrderStateInstance>()
            .Where(x => x.CorrelationId == createdSaga.Saga.CorrelationId)
            .FirstOrDefaultAsync();

            Assert.NotNull(dbSaga);
            Assert.Equal(sagaHarness.StateMachine.StockReserved.Name, dbSaga.CurrentState);
            // Verify saga transitioned to StockReserved state
            //var instance = sagaHarness.Created.ContainsInState(
            //    createdSaga.Saga.CorrelationId,
            //    sagaHarness.StateMachine,
            //    sagaHarness.StateMachine.StockReserved);
            //Assert.NotNull(instance);
        }
        finally
        {
            await testHarness.Stop();
        }
    }
}