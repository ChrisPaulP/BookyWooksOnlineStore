

using EventBus.IntegrationEventInterfaceAbstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EventBus.EventBusSubscriptionsManager.EventBusSubscriptionsManager;

namespace EventBus.EventBusSubscriptionsManager
{
    public interface IEventBusSubscriptionsManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnEventRemoved;

        void AddSubscription<T, TH>()
            where T : IntegrationEventBase
            where TH : IIntegrationEventHandler<T>;
        void RemoveSubscription<T, TH>()
            where T : IntegrationEventBase
            where TH : IIntegrationEventHandler<T>;
        

        bool HasSubscriptionsForEvent<T>() where T : IntegrationEventBase;
        bool HasSubscriptionsForEvent(string eventName);

        Type GetEventTypeByName(string eventName);
        void Clear();

        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEventBase;
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
           
        string GetEventKey<T>();
        
       
    }
}
