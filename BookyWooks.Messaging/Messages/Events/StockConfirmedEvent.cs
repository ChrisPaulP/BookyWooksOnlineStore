
using MassTransit;

namespace BookyWooks.Messaging.Contracts.Events;

public record StockConfirmedEvent(Guid CorrelationId, Guid orderId, IEnumerable<OrderItemEventDto> stockItems) : MessageContract(nameof(StockConfirmedEvent)), CorrelatedBy<Guid>;

//public record StockConfirmedEvent : IntegrationEvent, CorrelatedBy<Guid>
//{
//    public new Guid CorrelationId { get; init; }
//    public Guid OrderId { get; init; }
//    public IEnumerable<OrderItemEventDto> StockItems { get; init; }

//    public StockConfirmedEvent(Guid correlationId, Guid orderId, IEnumerable<OrderItemEventDto> stockItems)
//    {
//        CorrelationId = correlationId;
//        OrderId = orderId;
//        StockItems = stockItems;
//    }
//}
