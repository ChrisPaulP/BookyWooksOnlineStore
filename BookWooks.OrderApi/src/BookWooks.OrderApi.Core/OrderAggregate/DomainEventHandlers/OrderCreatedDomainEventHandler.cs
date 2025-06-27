using BookyWooks.Messaging.Constants;
using BookyWooks.Messaging.Contracts.Events;
using BookyWooks.Messaging.MassTransit;
using BookyWooks.Messaging.Messages.InitialMessage;

namespace BookWooks.OrderApi.Core.OrderAggregate.Handlers;
internal class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
{
    private readonly ILogger<OrderCreatedDomainEventHandler> _logger;

    public OrderCreatedDomainEventHandler(ILogger<OrderCreatedDomainEventHandler> logger) => _logger = logger;

    public Task Handle(OrderCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling Order Created Domain event for {new order}", domainEvent.NewOrder.Message);

    var orderStockList = domainEvent.NewOrder.OrderItems.Select(orderItem => new OrderItemEventDto(orderItem.OrderId.Value, orderItem.Quantity.Value));
    var orderCreatedMessage = new OrderCreatedMessage(domainEvent.NewOrder.OrderId.Value, domainEvent.NewOrder.CustomerId.Value, domainEvent.NewOrder.OrderTotal, orderStockList);

    return Task.CompletedTask;
    }
}
