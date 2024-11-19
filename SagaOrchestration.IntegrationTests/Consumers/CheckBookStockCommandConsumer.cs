using BookyWooks.Messaging.Contracts.Commands;
using BookyWooks.Messaging.Messages.InitialMessage;
using IntegrationTestingSetup;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaOrchestration.IntegrationTests.Consumers;

public class CheckBookStockCommandConsumer : IConsumer<CheckBookStockCommand>, ITestConsumer
{
    public async Task Consume(ConsumeContext<CheckBookStockCommand> context)
    {
        // TODO: Send email to customer
        await Task.Delay(1000);

    }
}
