using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.Core.OrderAggregate.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace BookWooks.OrderApi.Core;
public static class CoreDependencyInjection
{
  public static IServiceCollection AddCoreServices(this IServiceCollection services)
  {
    var assembly = typeof(OrderCreatedDomainEventHandler).Assembly;
    // Register MediatR services
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
    return services;
  }
}
