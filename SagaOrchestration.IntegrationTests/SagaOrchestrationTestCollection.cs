
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaOrchestration.IntegrationTests;
[CollectionDefinition("Saga Orchestration Test Collection")]
public class SagaOrchestrationTestCollection : ICollectionFixture<CustomSagaOrchestrationTestFactory<SagaOrchestrationProgram>>
{
    // No additional code needed, it inherits the generic behavior
}
