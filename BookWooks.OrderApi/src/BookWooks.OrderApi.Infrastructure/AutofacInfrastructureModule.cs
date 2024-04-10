namespace BookWooks.OrderApi.Infrastructure;
/// <summary>
/// An Autofac module responsible for wiring up services defined in Infrastructure.
/// Mainly responsible for setting up EF and MediatR, as well as other one-off services.
/// </summary>
public class AutofacInfrastructureModule : Module
{
  private readonly bool _isDevelopment = false;
  private readonly IConfiguration _configuration;

  public AutofacInfrastructureModule(bool isDevelopment, IConfiguration configuration)
  {
    _isDevelopment = isDevelopment;
    _configuration = configuration;
  }

  protected override void Load(ContainerBuilder builder)
  {
    if (_isDevelopment)
    {
      RegisterDevelopmentOnlyDependencies(builder);
    }
    else
    {
      RegisterProductionOnlyDependencies(builder);
    }
    RegisterDbContext(builder);
    RegisterEfReposotories(builder);
    RegisterMediatR(builder);
    RegisterMassTransitService(builder);
   // RegisterEventBusSubscriptionsManager(builder);
    RegisterLogger(builder);   
    RegisterDistributedCacheService(builder);
    RegisterRedisCache(builder);
    RegisterNewtonSoftService(builder);

  }
  private void RegisterDbContext(ContainerBuilder builder)
  {
    var connectionString = _configuration.GetConnectionString("DefaultConnection");

    builder.Register(c =>
    {
      var optionsBuilder = new DbContextOptionsBuilder<BookyWooksOrderDbContext>();
      optionsBuilder.UseSqlServer(connectionString);
      return optionsBuilder.Options;
    })
       .As<DbContextOptions<BookyWooksOrderDbContext>>()
       .SingleInstance(); // Registering DbContxtOptions here so it can be used in the SeedData class. If I did not need it in that class then this would not be needed.

    builder.Register(c =>
    {
      var optionsBuilder = new DbContextOptionsBuilder<BookyWooksOrderDbContext>();
      optionsBuilder.UseSqlServer(connectionString);
      var dispatcher = c.Resolve<IDomainEventDispatcher>();
      return new BookyWooksOrderDbContext(optionsBuilder.Options, dispatcher);
    })
 .AsSelf()
 .As<DbContext>() // Register as DbContext
 .InstancePerLifetimeScope();

    //builder.Register(c =>
    //{
    //  var optionsBuilder = new DbContextOptionsBuilder<IntegrationEventLogDbContext>();
    //  optionsBuilder.Use SqlServer(connectionString, sqlServerOptions =>
    //  {
    //    sqlServerOptions.MigrationsAssembly(typeof(IntegrationEventLogDbContext).Assembly.FullName);
    //    sqlServerOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null);
    //  });
    //  return new IntegrationEventLogDbContext(optionsBuilder.Options);
    //})
    //.InstancePerLifetimeScope();

  
  }
  private void RegisterEfReposotories(ContainerBuilder builder)
  {
    builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();
    builder.RegisterGeneric(typeof(ReadRepositoryDecorator<>)).As(typeof(IReadRepository<>));
  }
  private void RegisterMediatR(ContainerBuilder builder)
  {
   
    builder
    .RegisterType<Mediator>()
    .As<IMediator>()
    .InstancePerLifetimeScope();

    builder
    .RegisterType<MediatRDomainEventDispatcher<Guid>>()
    .As<IDomainEventDispatcher>()
    .InstancePerLifetimeScope();

    //builder
    // .RegisterGeneric(typeof(TransactionBehaviour<,>))
    // .As(typeof(IPipelineBehavior<,>))
    // .InstancePerLifetimeScope();

    builder
      .RegisterGeneric(typeof(LoggingBehavior<,>))
      .As(typeof(IPipelineBehavior<,>))
      .InstancePerLifetimeScope();
  }
  private void RegisterMassTransitService(ContainerBuilder builder)
  {
    //builder.Register(c =>
    //{
    //  var bookWooksOrderDbContext = c.Resolve<BookyWooksOrderDbContext>();
    //  var dbConnection = bookWooksOrderDbContext.Database.GetDbConnection();
    //  return new IntegrationEventLogService(dbConnection);
    //}).As<IIntegrationEventLogService>();

    builder.RegisterType<OrderMassTransitService>().As<IMassTransitService>().InstancePerLifetimeScope();
  }
  //public void RegisterEventBusSubscriptionsManager(ContainerBuilder builder)
  //{
  //  builder.RegisterType<EventBusSubscriptionsManager>()
  //         .As<IEventBusSubscriptionsManager>()
  //         .SingleInstance();
  //}
  public void RegisterLogger(ContainerBuilder builder)
  {
    builder.RegisterInstance(Log.Logger)
           .SingleInstance();
  }
  private void RegisterDistributedCacheService(ContainerBuilder builder)
  {
    builder.RegisterType<DistributedCacheService>().As<ICacheService>();
  }
  private void RegisterRedisCache(ContainerBuilder builder)
  {
#pragma warning disable CS8604 // Possible null reference argument.
    var redisConfiguration = new RedisCacheOptions
    {
      ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
      {
        AbortOnConnectFail = true,
        EndPoints = { _configuration.GetValue<string>("ConnectionStrings:Redis") }
      }
    };
#pragma warning restore CS8604 // Possible null reference argument.

    builder.RegisterInstance(redisConfiguration).As<RedisCacheOptions>();

    builder.Register(c =>
    {
      var options = c.Resolve<RedisCacheOptions>();
      return new RedisCache(options);
    }).As<IDistributedCache>().SingleInstance();

  }
  private void RegisterNewtonSoftService(ContainerBuilder builder)
  {
    builder.RegisterType<NewtonSoftService>().As<ISerializerService>();
  }
  private void RegisterDevelopmentOnlyDependencies(ContainerBuilder builder)
  {
    // NOTE: Add any development only services here
    builder.RegisterType<FakeEmailSender>().As<IEmailSender>()
      .InstancePerLifetimeScope();

    //builder.RegisterType<FakeListContributorsQueryService>()
    //  .As<IListContributorsQueryService>()
    //  .InstancePerLifetimeScope();

    builder.RegisterType<FakeGetOrdersByStatusQueryService>()
    .As<IGetOrdersByStatusQueryService>()
    .InstancePerLifetimeScope();

  }
  private void RegisterProductionOnlyDependencies(ContainerBuilder builder)
  {
    // NOTE: Add any production only (real) services here
    builder.RegisterType<SmtpEmailSender>().As<IEmailSender>()
      .InstancePerLifetimeScope();

    builder.RegisterType<GetOrdersByStatusQueryService>()
      .As<IGetOrderDetailsQueryService>()
      .InstancePerLifetimeScope();
  } 
}
