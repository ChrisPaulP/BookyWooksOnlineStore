using BookyWooks.Catalogue.Api.Data;
using BookyWooks.Catalogue.Api.Entities;
using BookyWooks.Messaging.Events;
using BookyWooks.Messaging.MassTransit;
using Marten;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BookyWooks.Catalogue.Api.Consumers;

public class CheckBookStockEventConsumer : IConsumer<CheckStockEvent>
{
    //private readonly IDocumentSession _session;
    private readonly CatalogueDbContext _context;
    private readonly ILogger<CheckBookStockEventConsumer> _logger;
    private readonly IMassTransitService _massTransitService;

    public CheckBookStockEventConsumer(ILogger<CheckBookStockEventConsumer> logger, CatalogueDbContext context, IMassTransitService massTransitService)
    {
        _massTransitService = massTransitService;
        _logger = logger;
        _context = context;
    }

    public async Task Consume(ConsumeContext<CheckStockEvent> @event)
    {
        _logger.LogInformation("----- Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Message.Id, @event);

        var stockItems = new List<OrderItemEventDto>();

        foreach (var orderItem in @event.Message.OrderItems)
        {
            //var product = _session.Query<Product>().Where(x => x.Id == orderItem.ProductId).FirstOrDefault();
            var product = _context.Products.Find(orderItem.ProductId);
            var hasStock = product.Quantity >= orderItem.quantity;
            var stockItem = new OrderItemEventDto(product.Id, product.Quantity, hasStock);

            stockItems.Add(stockItem);
        }

        var confirmedIntegrationEvent = stockItems.Any(c => !c.hasStock)
               ? (IntegrationEvent)new InsufficentStockEvent(@event.Message.OrderId, "There is not enough stock to complete your order", stockItems)
               : new StockConfirmedEvent(@event.Message.OrderId, stockItems);

        //_session.Events.Append(Guid.NewGuid(), confirmedIntegrationEvent);

        await _massTransitService.Send(confirmedIntegrationEvent); // change to publish in mass transit service
    }

}