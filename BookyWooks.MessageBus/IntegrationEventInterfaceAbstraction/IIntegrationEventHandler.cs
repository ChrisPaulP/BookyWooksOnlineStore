using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.IntegrationEventInterfaceAbstraction
{
    public interface IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IntegrationEventBase
    {
        Task Handle(TIntegrationEvent @event);
    }

}
