using BookyWooks.Catalogue.Api.Data;
using BookyWooks.Catalogue.Api.Entities;
using BookyWooks.Messaging.Constants;
using BookyWooks.Messaging.Contracts.Commands;
using BookyWooks.Messaging.Contracts.Events;
using BookyWooks.Messaging.MassTransit;

using Marten;
using MassTransit;
using MassTransit.Transports;
using Microsoft.EntityFrameworkCore;

namespace BookyWooks.Catalogue.Api.Consumers;

public class CheckBookStockCommandConsumer : IConsumer<CheckBookStockCommand>
{
    private readonly CatalogueDbContext _context;
    private readonly ILogger<CheckBookStockCommandConsumer> _logger;
    private readonly IMassTransitService _massTransitService;

    public CheckBookStockCommandConsumer(ILogger<CheckBookStockCommandConsumer> logger, CatalogueDbContext context, IMassTransitService massTransitService)
    {
        _massTransitService = massTransitService;
        _logger = logger;
        _context = context;
    }

    public async Task Consume(ConsumeContext<CheckBookStockCommand> @event)
    {
        //_logger.LogInformation("----- Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Message.Id, @event);

        var stockItems = new List<OrderItemEventDto>();

        foreach (var orderItem in @event.Message.orderItems)
        {
            //var product = _session.Query<Product>().Where(x => x.Id == orderItem.ProductId).FirstOrDefault();
            var product = _context.Products.Find(orderItem.ProductId);
            var hasStock = product.Quantity >= orderItem.quantity;
            var stockItem = new OrderItemEventDto(product.Id, product.Quantity, hasStock);
            if(hasStock) product.Quantity -= orderItem.quantity;
            stockItems.Add(stockItem);
        }

        if (stockItems.Any(c => !c.hasStock))
        {
            var stockReservationFailedEvent = new StockReservationFailedEvent(@event.Message.CorrelationId, @event.Message.orderId, "There is not enough stock to complete your order", stockItems);
            await _massTransitService.Publish(stockReservationFailedEvent);
        }
        else
        {
            await _context.SaveChangesAsync();
            var stockConfirmedEvent = new StockConfirmedEvent(@event.Message.CorrelationId, @event.Message.orderId, stockItems);
            await _massTransitService.Publish(stockConfirmedEvent);
        }

    }
}