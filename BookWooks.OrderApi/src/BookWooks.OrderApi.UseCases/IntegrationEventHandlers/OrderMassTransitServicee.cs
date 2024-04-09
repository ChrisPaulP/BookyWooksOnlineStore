using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.Core.OrderAggregate.IntegrationEvents;

using BookyWooks.Messaging.MassTransit;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BookWooks.OrderApi.UseCases.IntegrationEventHandlers;
public class OrderMassTransitServicee : IMassTransitService
{
  private readonly IPublishEndpoint _publishEndpoint;
  private readonly ILogger<OrderMassTransitServicee> _logger;
  public OrderMassTransitServicee(

      ILogger<OrderMassTransitServicee> logger,
      IPublishEndpoint publishEndpoint)

  {
    _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }
  public async Task Send<T>(T message) where T : class
  {
    await _publishEndpoint.Publish(message);
  }
}
