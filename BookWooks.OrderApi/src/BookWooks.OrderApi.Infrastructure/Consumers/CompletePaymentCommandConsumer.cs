namespace BookWooks.OrderApi.Infrastructure.Consumers;
public class CompletePaymentCommandConsumer : InboxConsumer<CompletePaymentCommand, BookyWooksOrderDbContext>
{
  private readonly ILogger<CompletePaymentCommandConsumer> _logger;
  private readonly ICommandScheduler _commandsScheduler;

  public CompletePaymentCommandConsumer(IServiceScopeFactory serviceScopeFactory, ILogger<CompletePaymentCommandConsumer> logger, ICommandScheduler commandsScheduler)
      : base(serviceScopeFactory)
  {
    _logger = logger;
    _commandsScheduler = commandsScheduler;
  }

  public override async Task Consume(CompletePaymentCommand message)
  {
    await _commandsScheduler.EnqueueAsync(new CompletePaymentInternalCommand(Guid.NewGuid(), message.CustomerId));

    _logger.LogInformation("CompletePaymentCommand saved for CustomerId: {CustomerId}, OrderTotal: {OrderTotal}",
        message.CustomerId, message.OrderTotal);

    //// TODO: Send email to customer
    //await Task.Delay(1000);
    //_logger.LogInformation("Order Failed notification sent to customer with Id: {CustomerId} for order Id: {MessageOrderId}",
    //   message.customerId, message.orderTotal);
  }
}
