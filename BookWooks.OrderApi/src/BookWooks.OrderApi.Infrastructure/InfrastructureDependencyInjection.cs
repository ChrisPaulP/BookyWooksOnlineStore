using BookWooks.OrderApi.Infrastructure.Common.Behaviour;
using BookWooks.OrderApi.UseCases.Create;

namespace BookWooks.OrderApi.Infrastructure;
public static class InfrastructureDependencyInjection
{
  public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,IConfiguration configuration,bool isDevelopment,BiDirectionalDictionary<string, Type>? domainEventsMap = null,BiDirectionalDictionary<string, Type>? internalCommandMap = null, bool isTest = false)
  {
        domainEventsMap ??= new BiDirectionalDictionary<string, Type>();
        internalCommandMap ??= new BiDirectionalDictionary<string, Type>();
                
        RegisterMassTransit(services);      
        RegisterSerializer(services);
        RegisterMessageBroker(services, configuration);
        RegisterDbContext(services, configuration);
        RegisterOutboxContext(services);
        RegisterDomainEventsWrapper(services, domainEventsMap);
        RegisterDomainEventsDispatcherNotificationHandlerDecorator(services);
        RegisterInternalCommandMapper(services, internalCommandMap);
        RegisterCommandScheduler(services);
        RegisterInboxContext(services);
        RegisterEfRepositories(services);
        RegisterMediatR(services);
        RegisterDistributedCacheService(services);
        RegisterRedisDistributedCache(services, configuration);
        RegisterEnvironmentSpecificDependencies(services, isDevelopment);
        RegisterQuartz(services);
        if (!isTest)
        {
          RegisterAIOptions(services);
          RegisterAIService(services);
        }

    return services;
    }

  private static void RegisterAIService(IServiceCollection services)
  {
    services.AddScoped<IOrderAiService<ProductDto>, OrderAiService>();
  }

  private static void RegisterQuartz(IServiceCollection services)
  {
    services.AddQuartz(q =>
    {
      q.ScheduleJob<ProcessOutboxJob>(trigger => trigger
          .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(10)).RepeatForever())
          .StartNow()
      );
      q.ScheduleJob<ProcessInternalCommandJob>(trigger => trigger
          .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(10)).RepeatForever())
          .StartNow()
      );
    });

    services.AddQuartzHostedService(options =>
    {
      options.WaitForJobsToComplete = true;
    });
  }

  private static void RegisterMassTransit(IServiceCollection services)
  {
    services.AddScoped<IMassTransitService, OrderMassTransitService>();
  }

  private static void RegisterDistributedCacheService(IServiceCollection services)
  {
    services.AddScoped<ICacheService, DistributedCacheService>();
  }

  private static void RegisterSerializer(IServiceCollection services)
  {
    services.AddScoped<ISerializerService, NewtonSoftService>();
  }

  private static void RegisterMessageBroker(IServiceCollection services, IConfiguration configuration)
  {
    services.AddMessageBroker<BookyWooksOrderDbContext>(configuration, Assembly.GetExecutingAssembly(), true);
  }

  private static void RegisterAIOptions(IServiceCollection services)
  {
    services.AddOptions<OpenAIOptions>()
            .BindConfiguration(OpenAIOptions.Key)
            .ValidateDataAnnotations()  
            .ValidateOnStart();
  }

  private static void RegisterDomainEventsDispatcherNotificationHandlerDecorator(IServiceCollection services)
  {
    services.AddTransient(typeof(INotificationHandler<>), typeof(DomainEventsDispatcherNotificationHandlerDecorator<>));
    // this is needed for the rare occasions when a NotificationHandler such as OrderCreatedDomainEventHandler does not save chnages but domain events still need to be dispatched. 
    //Note OrderCreatedDomainEventHandler is an example of a class that implements INotificationHandler but on this occassion no domain events need to be raised.
    //However if the were this decorator would take care of dispatching them.
  }

  private static void RegisterCommandScheduler(IServiceCollection services)
  {
    services.AddScoped<ICommandScheduler, CommandScheduler>();
  }

  private static void RegisterDbContext(IServiceCollection services, IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("OrderDatabase");

    services.AddDbContext<BookyWooksOrderDbContext>(options =>
        options.UseSqlServer(connectionString));
    services.AddScoped<DbContext, BookyWooksOrderDbContext>();
  }
  private static void RegisterOutboxContext(IServiceCollection services)
  {
    services.AddScoped<IOutboxDbContext, BookyWooksOrderDbContext>();
    services.AddScoped<IOutbox, OutboxMethods>();
  }
  private static void RegisterDomainEventsWrapper(IServiceCollection services, BiDirectionalDictionary<string, Type> domainEventsMap)
  {
    CheckDomainEventMappings(domainEventsMap);
    services.AddSingleton<IDomainEventMapper>(provider =>
    {
      return new DomainEventMapper(domainEventsMap);
    });
  }
  private static void RegisterInternalCommandMapper(IServiceCollection services, BiDirectionalDictionary<string, Type> internalCommandMap)
  {
    CheckInternalCommandMappings(internalCommandMap);
    services.AddSingleton<IInternalCommandMapper>(provider =>
    {
      return new InternalCommandMapper(internalCommandMap);
    });
  }
  private static void RegisterInboxContext(IServiceCollection services)
  {
    services.AddScoped<IInboxDbContext, BookyWooksOrderDbContext>();
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
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));
    var assemblies = new[]
   {
        typeof(ProcessInternalCommand).Assembly,
        typeof(CreateOrderCommand).Assembly
    };
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
  }

  private static void RegisterRedisDistributedCache(IServiceCollection services, IConfiguration configuration)
  {
        var redisConnectionString = configuration.GetValue<string>("ConnectionStrings:Redis");
    var redis = "localhost:6379"; // Default Redis connection string, can be overridden by configuration
    var redisConfiguration = new RedisCacheOptions
        {
            ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
            {
                AbortOnConnectFail = true,
                EndPoints = { redis ?? throw new ArgumentNullException(nameof(redisConnectionString)) }
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
  }

    private static void RegisterProductionOnlyDependencies(IServiceCollection services)
    {
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IGetOrdersByStatusQueryService, GetOrdersByStatusQueryService>();
    }
    private static void CheckDomainEventMappings(BiDirectionalDictionary<string, Type> domainNotificationsMap)
    {
        var domainEvents = Assemblies.UseCasesAssembly
            .GetTypes()
            .Where(type => typeof(DomainEventBase).IsAssignableFrom(type) && !type.IsAbstract)
            .ToList();

        var notMappedDomainEvents = domainEvents
            .Where(domainEvent => !domainNotificationsMap.TryGetBySecond(domainEvent, out var name) || string.IsNullOrEmpty(name))
            .ToList();

        if (notMappedDomainEvents.Any())
        {
            throw new ApplicationException(
                $"Domain Event Notifications {string.Join(",", notMappedDomainEvents.Select(x => x.FullName))} not mapped");
        }
    }

    private static void CheckInternalCommandMappings(BiDirectionalDictionary<string, Type> internalCommandMap)
    {
        var internalCommands = Assemblies.UseCasesAssembly
            .GetTypes()
            .Where(x => x.BaseType != null &&
                        (
                            (x.BaseType.IsGenericType &&
                            x.BaseType.GetGenericTypeDefinition() == typeof(InternalCommandBase<>)) ||
                            x.BaseType == typeof(InternalCommandBase)))
            .ToList();

        var notMappedInternalCommands = internalCommands
            .Where(internalCommand => !internalCommandMap.TryGetBySecond(internalCommand, out _))
            .ToList();

        if (notMappedInternalCommands.Any())
        {
            throw new ApplicationException($"Internal Commands {string.Join(",", notMappedInternalCommands.Select(x => x.FullName))} not mapped");
        }
    }
  public static IServiceCollection AddInfrastructureServicesForMCPServer(this IServiceCollection services,IConfiguration configuration,bool isDevelopment)
  {
    RegisterDbContext(services, configuration);
    RegisterEfRepositories(services);
    RegisterSerializer(services);
    RegisterDistributedCacheService(services);
    RegisterRedisDistributedCache(services, configuration);
    return services;
  }
}

