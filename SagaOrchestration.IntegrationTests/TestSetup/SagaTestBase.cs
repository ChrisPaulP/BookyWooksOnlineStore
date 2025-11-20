namespace SagaOrchestration.IntegrationTests.TestSetup;

public abstract class SagaTestBase : IAsyncLifetime
{
    protected readonly CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram> Factory;

    protected SagaTestBase(CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram> factory)
    {
        Factory = factory;
    }
    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        await CleanDatabaseAsync();
    }

    protected async Task CleanDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StateMachineDbContext>();
        
        context.Set<InboxState>().RemoveRange(context.Set<InboxState>());
        context.Set<OutboxMessage>().RemoveRange(context.Set<OutboxMessage>());
        context.Set<OutboxState>().RemoveRange(context.Set<OutboxState>());
        context.Set<OrderStateInstance>().RemoveRange(context.Set<OrderStateInstance>());
        
        await context.SaveChangesAsync();
    }

    protected async Task<List<T>> FindAllAsync<T>() where T : class
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StateMachineDbContext>();
        return await context.Set<T>().ToListAsync();
    }

    protected static OrderCreatedMessage CreateTestOrderMessage()
    {
        var orderItems = new List<OrderItemEventDto>
        {
            new OrderItemEventDto(Guid.NewGuid(), 1, true)
        };

        return new OrderCreatedMessage(
            orderId: Guid.NewGuid(),
            customerId: Guid.NewGuid(),
            orderTotal: 9.99M,
            orderItems: orderItems
        );
    }

    // Helper method to get a fresh test harness for each test
    protected ITestHarness GetTestHarness()
    {
        return Factory.Services.GetRequiredService<ITestHarness>();
    }

    // Helper method to get a fresh saga harness for each test
    protected ISagaStateMachineTestHarness<OrderStateMachine, OrderStateInstance> GetSagaHarness()
    {
        var harness = GetTestHarness();
        return harness.GetSagaStateMachineHarness<OrderStateMachine, OrderStateInstance>();
    }
}