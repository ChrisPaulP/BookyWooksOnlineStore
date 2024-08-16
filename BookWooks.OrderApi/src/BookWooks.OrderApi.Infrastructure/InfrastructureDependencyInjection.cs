namespace BookWooks.OrderApi.Infrastructure;
public static class InfrastructureDependencyInjection
{
  public static IServiceCollection AddInfrastructureServices
        (this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
  {
    services.AddScoped<IMassTransitService, OrderMassTransitService>();
    services.AddScoped<ICacheService, DistributedCacheService>();
    services.AddScoped<ISerializerService, NewtonSoftService>();
    services.AddMessageBroker<BookyWooksOrderDbContext>(configuration, Assembly.GetExecutingAssembly(), true);
    RegisterDbContext(services, configuration);
    RegisterEfRepositories(services);
    RegisterMediatR(services);
    RegisterRedisCache(services, configuration);
    RegisterEnvironmentSpecificDependencies(services, isDevelopment);
    return services;
  }

  private static void RegisterDbContext(IServiceCollection services, IConfiguration configuration)
  {
    //var connectionString = configuration.GetConnectionString("DefaultConnection");

    //services.AddDbContext<BookyWooksOrderDbContext>(options =>
    //    options.UseSqlServer(connectionString));

    // Quick Note. The commented out code above can be used, however I prefer the below way as its more readable and clear.
    services.AddScoped<BookyWooksOrderDbContext>(provider =>
    {
      var connectionString = configuration.GetConnectionString("OrderDatabase");

      var optionsBuilder = new DbContextOptionsBuilder<BookyWooksOrderDbContext>();
      optionsBuilder.UseSqlServer(connectionString);

      var options = optionsBuilder.Options;
      var dispatcher = provider.GetRequiredService<IDomainEventDispatcher>();
      return new BookyWooksOrderDbContext(options, dispatcher);
    });
  }

  private static void RegisterEfRepositories(IServiceCollection services)
  {
    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepositoryDecorator<>));
  }

  private static void RegisterMediatR(IServiceCollection services)
  {
    services.AddScoped<IMediator, Mediator>();
    services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher<Guid>>();
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
  }

  private static void RegisterRedisCache(IServiceCollection services, IConfiguration configuration)
  {
        var redisConnectionString = configuration.GetValue<string>("ConnectionStrings:Redis");
        var redisConfiguration = new RedisCacheOptions
        {
            ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
            {
                AbortOnConnectFail = true,
                EndPoints = { redisConnectionString ?? throw new ArgumentNullException(nameof(redisConnectionString)) }
            }
        };
    services.AddSingleton(redisConfiguration);
    services.AddSingleton<IDistributedCache>(sp =>
    {
      var options = sp.GetRequiredService<RedisCacheOptions>();
      return new RedisCache(options);
    });
  }
  private static void RegisterEnvironmentSpecificDependencies(IServiceCollection services, bool isDevelopment)
  {
    if (isDevelopment)
    {
      RegisterDevelopmentOnlyDependencies(services);
    }
    else
    {
      RegisterProductionOnlyDependencies(services);
    }
  }
  private static void RegisterDevelopmentOnlyDependencies(IServiceCollection services)
  {
    services.AddScoped<IEmailSender, FakeEmailSender>();
    //services.AddScoped<IGetOrdersByStatusQueryService, FakeGetOrdersByStatusQueryService>();
    services.AddScoped<IGetOrdersByStatusQueryService, GetOrdersByStatusQueryService>();
    // Uncomment and adapt if needed
    // services.AddScoped<IListContributorsQueryService, FakeListContributorsQueryService>();
  }

  private static void RegisterProductionOnlyDependencies(IServiceCollection services)
  {
    services.AddScoped<IEmailSender, SmtpEmailSender>();
    services.AddScoped<IGetOrdersByStatusQueryService, GetOrdersByStatusQueryService>();
  }
}

