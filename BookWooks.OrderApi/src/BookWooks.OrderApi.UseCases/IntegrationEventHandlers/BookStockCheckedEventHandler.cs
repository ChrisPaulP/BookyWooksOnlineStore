

using BookWooks.OrderApi.Core.OrderAggregate.IntegrationEvents;
using MassTransit;

namespace BookWooks.OrderApi.UseCases.IntegrationEventHandlers;
public class BookStockCheckedEventHandler : IConsumer<CheckBookStockIntegrationEvent>, IIntegrationEventHandler// IIntegrationEventHandler<CheckBookStockIntegrationEvent>
{
  public async Task Consume(ConsumeContext<CheckBookStockIntegrationEvent> @event)
  {
    // CHRIS !!!!!!
    // Handle IntegrationEventB
    await Task.CompletedTask;

  }
}
