

using EventBus.IntegrationEventInterfaceAbstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus;

public interface IEventBus
{
    Task Publish(IntegrationEventBase _integrationEvent);

    Task Subscribe<T, TH>() where T : IntegrationEventBase where TH : IIntegrationEventHandler<T>;


}
