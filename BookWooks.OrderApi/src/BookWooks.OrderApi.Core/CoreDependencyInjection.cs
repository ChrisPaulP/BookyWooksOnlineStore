
using Microsoft.Extensions.DependencyInjection;

namespace BookWooks.OrderApi.Core;
public static class CoreDependencyInjection
{
  public static IServiceCollection AddCoreServices(this IServiceCollection services)
  {
    var assembly = typeof(OrderFulfilledDomainEvent).Assembly;
    // Register MediatR services
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
    return services;
  }
}
