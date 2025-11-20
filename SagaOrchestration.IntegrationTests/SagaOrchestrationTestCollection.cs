using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaOrchestration.IntegrationTests;

[CollectionDefinition("Saga Orchestration Test Collection", DisableParallelization = true)]
public class SagaOrchestrationTestCollection : ICollectionFixture<CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram>>
{
    // No additional code needed, it inherits the generic behavior
}

[CollectionDefinition("Order Created Consumed Test Collection", DisableParallelization = true)]
public class OrderCreatedConsumedTestCollection : ICollectionFixture<CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram>>
{
    // Separate collection for OrderCreatedConsumedByStateMachineTests
}

[CollectionDefinition("Order Created Outbox Test Collection", DisableParallelization = true)]
public class OrderCreatedOutboxTestCollection : ICollectionFixture<CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram>>
{
    // Separate collection for OrderCreatedOutboxPatternTests
}

[CollectionDefinition("Order Created Publish To Outbox Test Collection", DisableParallelization = true)]
public class OrderCreatedPublishToOutboxTestCollection : ICollectionFixture<CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram>>
{
    // Separate collection for OrderCreatedPublishToOutboxTests
}

[CollectionDefinition("Order Created Send To Consumer Test Collection", DisableParallelization = true)]
public class OrderCreatedSendToConsumerTestCollection : ICollectionFixture<CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram>>
{
    // Separate collection for OrderCreatedSendToConsumerTests
}

[CollectionDefinition("Order Created Transition To Stock Check Test Collection", DisableParallelization = true)]
public class OrderCreatedTransitionToStockCheckTests : ICollectionFixture<CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram>>
{
    // Separate collection for OrderCreatedTransitionToStockCheckTests
}


[CollectionDefinition("Stock Confirmed Saga State Test Collection", DisableParallelization = true)]
public class StockConfirmedSagaStateTests : ICollectionFixture<CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram>>
{
    // Separate collection for StockConfirmedSagaStateTests
}
