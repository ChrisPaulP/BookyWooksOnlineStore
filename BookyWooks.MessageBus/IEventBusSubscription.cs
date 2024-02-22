
using EventBus.IntegrationEventInterfaceAbstraction;

namespace EventBus;

public interface IEventBusSubscription
{
    void Subscribe<T, TH>()
           where T : IntegrationEventBase
           where TH : IIntegrationEventHandler<T>;

    void Unsubscribe<T, TH>()
        where TH : IIntegrationEventHandler<T>
        where T : IntegrationEventBase;
}
