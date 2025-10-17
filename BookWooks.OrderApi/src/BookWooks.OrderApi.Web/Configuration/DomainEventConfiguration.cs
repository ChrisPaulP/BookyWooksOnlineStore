using BookWooks.OrderApi.Core.OrderAggregate.Events;
using BookyWooks.Messaging.Messages.InitialMessage;
using BookyWooks.SharedKernel;

namespace BookWooks.OrderApi.Web.Configuration;

public static class DomainEventConfiguration
{
    public static BiDirectionalDictionary<string, Type> InitializeDomainEventsMap()
    {
        var map = new BiDirectionalDictionary<string, Type>();
        map.Add("OrderCreatedEvent", typeof(OrderCreatedDomainEvent));
        map.Add("OrderCreatedMessage", typeof(OrderCreatedMessage));
        return map;
    }

    public static BiDirectionalDictionary<string, Type> InitializeInternalCommandMap()
    {
        var map = new BiDirectionalDictionary<string, Type>();
        map.Add("CompletePaymentInternalCommand", typeof(CompletePaymentInternalCommand));
        return map;
    }
}
