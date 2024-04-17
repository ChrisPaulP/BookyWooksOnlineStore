namespace BookWooks.OrderApi.Infrastructure.Consumers;

public class BookStockCheckedEventConsumer : IConsumer<CheckStockEvent>
{
public async Task Consume(ConsumeContext<CheckStockEvent> @event)
{
  // CHRIS !!!!!!
  // Handle IntegrationEventB
  await Task.CompletedTask;

}
}
