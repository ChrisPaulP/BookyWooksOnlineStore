using System.Reflection;
using Autofac;

using BookWooks.OrderApi.Core.Interfaces;
using BookWooks.OrderApi.Core.OrderAggregate;
using BookWooks.OrderApi.Infrastructure.Caching;
using BookWooks.OrderApi.Infrastructure.Data.Queries;
using BookWooks.OrderApi.Infrastructure.Data.Repositories;
using BookWooks.OrderApi.Infrastructure.Data.Repositories.Abstract;
using BookWooks.OrderApi.Infrastructure.Email;

using BookWooks.OrderApi.UseCases.Create;
using BookWooks.OrderApi.UseCases.Orders;
using BookWooks.OrderApi.UseCases.Orders.Get;
using BookWooks.OrderApi.UseCases.Orders.List;
using BookyWooks.SharedKernel;
using EventBus.EventBusSubscriptionsManager;
using EventBus;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Serilog;
using Module = Autofac.Module;
using Humanizer.Configuration;
using Humanizer;
using RabbitMQ.Client;
using BookWooks.OrderApi.Infrastructure.Common.Behaviour;
using BookWooks.OrderApi.Infrastructure.IntegrationEventService;

using System.Data.Common;
using BookWooks.OrderApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BookWooks.OrderApi.UseCases.Contributors.Create;
using EventBus.IntegrationEventInterfaceAbstraction;
using BookWooks.OrderApi.UseCases.IntegrationEventHandlers;
using BookWooks.OrderApi.Core.Orders;


using OutBoxPattern.IntegrationEventLogServices;
using OutBoxPattern;
using Microsoft.Extensions.DependencyModel;


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
    RegisterIntegrationEventServices(builder);
    RegisterEventBusSubscriptionsManager(builder);
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
      var dispatcher = c.Resolve<IDomainEventDispatcher<Guid>>();
      return new BookyWooksOrderDbContext(optionsBuilder.Options, dispatcher);
    })
 .AsSelf()
 .As<DbContext>() // Register as DbContext
 .InstancePerLifetimeScope();

    builder.Register(c =>
    {
      var optionsBuilder = new DbContextOptionsBuilder<IntegrationEventLogDbContext>();
      optionsBuilder.UseSqlServer(connectionString, sqlServerOptions =>
      {
        sqlServerOptions.MigrationsAssembly(typeof(BookyWooksOrderDbContext).Assembly.FullName);
        sqlServerOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null);
      });
      return new IntegrationEventLogDbContext(optionsBuilder.Options);
    })
    .InstancePerLifetimeScope();


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
    .As<IDomainEventDispatcher<Guid>>()
    .InstancePerLifetimeScope();

    builder
     .RegisterGeneric(typeof(TransactionBehaviour<,>))
     .As(typeof(IPipelineBehavior<,>))
     .InstancePerLifetimeScope();

    builder
      .RegisterGeneric(typeof(LoggingBehavior<,>))
      .As(typeof(IPipelineBehavior<,>))
      .InstancePerLifetimeScope();
  }
  private void RegisterIntegrationEventServices(ContainerBuilder builder)
  {
    builder.Register(c =>
    {
      var bookWooksOrderDbContext = c.Resolve<BookyWooksOrderDbContext>();
      var dbConnection = bookWooksOrderDbContext.Database.GetDbConnection();
      return new IntegrationEventLogService(dbConnection);
    }).As<IIntegrationEventLogService>();

    builder.RegisterType<OrderIntegrationEventService>().As<IOrderIntegrationEventService>(); ;
  }
  public void RegisterEventBusSubscriptionsManager(ContainerBuilder builder)
  {
    builder.RegisterType<EventBusSubscriptionsManager>()
           .As<IEventBusSubscriptionsManager>()
           .SingleInstance();
  }
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
    var redisConfiguration = new RedisCacheOptions
    {
      ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
      {
        AbortOnConnectFail = true,
        EndPoints = { _configuration.GetValue<string>("ConnectionStrings:Redis") }
      }
    };

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
