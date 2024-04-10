namespace BookWooks.OrderApi.Infrastructure;
public static class InfrastructureDependencyInjection
{
  public static IServiceCollection AddInfrastructureMessagingServices
        (this IServiceCollection services, IConfiguration configuration)
  {
    services.AddMessageBroker<BookyWooksOrderDbContext>(configuration, Assembly.GetExecutingAssembly());

    return services;
  }
}
