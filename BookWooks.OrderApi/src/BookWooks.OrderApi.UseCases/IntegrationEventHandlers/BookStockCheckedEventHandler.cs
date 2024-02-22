

using BookWooks.OrderApi.Core.OrderAggregate.IntegrationEvents;
using EventBus.IntegrationEventInterfaceAbstraction;

namespace BookWooks.OrderApi.UseCases.IntegrationEventHandlers;
public class BookStockCheckedEventHandler : IIntegrationEventHandler<CheckBookStockIntegrationEvent>
{
  public async Task Handle(CheckBookStockIntegrationEvent @event)
  {
    // Handle IntegrationEventB
    await Task.CompletedTask;
  }
}
