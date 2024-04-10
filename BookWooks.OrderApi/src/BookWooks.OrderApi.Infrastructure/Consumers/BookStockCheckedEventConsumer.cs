namespace BookWooks.OrderApi.Infrastructure.Consumers;

public class BookStockCheckedEventConsumer : IConsumer<CheckBookStockIntegrationEvent>
{
public async Task Consume(ConsumeContext<CheckBookStockIntegrationEvent> @event)
{
  // CHRIS !!!!!!
  // Handle IntegrationEventB
  await Task.CompletedTask;

}
}
