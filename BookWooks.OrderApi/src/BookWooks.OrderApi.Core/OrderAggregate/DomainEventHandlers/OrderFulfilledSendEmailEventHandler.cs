namespace BookWooks.OrderApi.Core.OrderAggregate.Handlers;
public class OrderFulfilledSendEmailEventHandler : INotificationHandler<OrderFulfilledEvent>
{
  private readonly IEmailSender _emailSender;


  // In a REAL app you might want to use the Outbox pattern and a command/queue here...
  public OrderFulfilledSendEmailEventHandler(IEmailSender emailSender)
  {
    _emailSender = emailSender;
  }

  // configure a test email server to demo this works
  // https://ardalis.com/configuring-a-local-test-email-server
  public Task Handle(OrderFulfilledEvent domainEvent, CancellationToken cancellationToken)
  {
    Guard.Against.Null(domainEvent, nameof(domainEvent));

    return _emailSender.SendEmailAsync("test@test.com", "test@test.com", $"{domainEvent.FulfilledOrder.Message} was completed.", domainEvent.FulfilledOrder.Status.Name);
  }
}
