using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BookWooks.OrderApi.UseCases.IntegrationEventHandlers;
public class TestIntegrationEventHandler : IConsumer<TestIntegrationEvent>, IIntegrationEventHandler
{
  private readonly ILogger<TestIntegrationEventHandler> _logger;

  public TestIntegrationEventHandler(ILogger<TestIntegrationEventHandler> logger)
  {
    _logger = logger;
  }

  public Task Consume(ConsumeContext<TestIntegrationEvent> context)
  {
    _logger.LogInformation(context.Message.ToString());
    return Task.CompletedTask;
  }
}
