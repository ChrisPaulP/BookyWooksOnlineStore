
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using BookyWooks.Messaging.RabbitMq;
using Microsoft.Extensions.Options;
using MassTransit;
using BookWooks.OrderApi.Infrastructure.Data;
using BookWooks.OrderApi.UseCases.IntegrationEventHandlers;
using BookyWooks.Messaging.MassTransit;
namespace BookWooks.OrderApi.Infrastructure;
public static class InfrastructureDependencyInjection
{
  public static IServiceCollection AddInfrastructureMessagingServices
        (this IServiceCollection services, IConfiguration configuration)
  {
    var consumerType = typeof(IIntegrationEventHandler);
    Assembly assembly = consumerType.Assembly;
    services.AddMessageBroker<BookyWooksOrderDbContext>(configuration, assembly);

    return services;
  }
}
