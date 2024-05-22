using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookyWooks.Messaging.Contracts.Commands;


namespace BookWooks.OrderApi.Infrastructure.Consumers;
public class CompletePaymentCommandConsumer : IConsumer<CompletePaymentCommand>
{
  private readonly ILogger<CompletePaymentCommand> _logger;

  public CompletePaymentCommandConsumer(ILogger<CompletePaymentCommand> logger)
  {
    _logger = logger;
  }

  public async Task Consume(ConsumeContext<CompletePaymentCommand> context)
  {
    // TODO: Send email to customer
    await Task.Delay(1000);
    _logger.LogInformation("Order Failed notification sent to customer with Id: {CustomerId} for order Id: {MessageOrderId}",
       context.Message.customerId, context.Message.orderTotal);
  }
}
