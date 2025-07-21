using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace IntegrationTestingSetup;

public abstract class TestFactoryBase<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAsyncLifetime where TEntryPoint : class
{
    protected readonly MsSqlContainer _mssqlContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;
    //private readonly RedisContainer _redisContainer;  
    private const string RabbitMqUsername = "guest";
    private const string RabbitMqPassword = "guest";
    protected IConfiguration Configuration { get; private set; }

    protected TestFactoryBase()
    {
        _mssqlContainer = IntegrationTestingSetupExtensions.CreateMsSqlContainer();
        _rabbitMqContainer = IntegrationTestingSetupExtensions.CreateRabbitMqContainer();
        //_redisContainer = IntegrationTestingSetupExtensions.CreateRedisContainer();
    }

    public async Task InitializeAsync()
    {
        await IntegrationTestingSetupExtensions.StartContainersAsync(_mssqlContainer, _rabbitMqContainer); //, _redisContainer);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            Configuration = new ConfigurationBuilder().AddJsonFile("testcontainersappsettings.json", optional: true)
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = _mssqlContainer.GetConnectionString(),
                    ["ConnectionStrings:SagaOrchestrationDatabase"] = _mssqlContainer.GetConnectionString(),
                    ["ConnectionStrings:OrderDatabase"] = _mssqlContainer.GetConnectionString(),
                    ["RabbitMQConfiguration:Config:HostName"] = _rabbitMqContainer.Hostname
                })
                .AddEnvironmentVariables()
                .Build();

            //var redisConn = $"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort(6379)}";
            //Console.WriteLine($"[DEBUG] Redis Testcontainers connection string: {redisConn}");
            //Configuration = new ConfigurationBuilder()
            //        .AddEnvironmentVariables()
            //        .AddInMemoryCollection(new Dictionary<string, string>
            //        {
            //            ["ConnectionStrings:DefaultConnection"] = _mssqlContainer.GetConnectionString(),
            //            ["ConnectionStrings:SagaOrchestrationDatabase"] = _mssqlContainer.GetConnectionString(),
            //            ["RabbitMQConfiguration:Config:HostName"] = _rabbitMqContainer.Hostname,
            //            ["RabbitMQConfiguration:Config:UserName"] = RabbitMqUsername,
            //            ["RabbitMQConfiguration:Config:Password"] = RabbitMqPassword,
            //            ["ConnectionStrings:Redis"] = redisConn
            //        })
            //        //.AddEnvironmentVariables()
            //        .Build();

            configurationBuilder.AddConfiguration(Configuration);
        });

        builder.ConfigureTestServices(services =>
        {
            //OverrideRedis(services, Configuration);
            services.AddMassTransitTestHarness(busRegistrationConfigurator =>
            {
                ConfigureMassTransit(busRegistrationConfigurator);

                busRegistrationConfigurator.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(_rabbitMqContainer.GetConnectionString()), h =>
                    {
                        h.Username(RabbitMqUsername);
                        h.Password(RabbitMqPassword);
                    });
                    ConfigureEndpoints(context, cfg);
                });
            });

            //ConfigureProjectSpecificServices(services);
        });
    }
    private static void OverrideRedis(IServiceCollection services, IConfiguration configuration)
    {
        // Remove existing Redis registrations (from Startup)
        var descriptors = services
            .Where(s => s.ServiceType == typeof(IDistributedCache) || s.ServiceType == typeof(RedisCacheOptions))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }

        // Re-register using Testcontainers connection string
        var redisConnectionString = configuration.GetValue<string>("ConnectionStrings:Redis")
            ?? throw new ArgumentNullException("Redis connection string not configured");

        Console.WriteLine($"[DEBUG] Overriding Redis with: {redisConnectionString}");

        var redisConfiguration = new RedisCacheOptions
        {
            ConfigurationOptions = new ConfigurationOptions
            {
                AbortOnConnectFail = true,
                EndPoints = { redisConnectionString }
            }
        };

        services.AddSingleton(redisConfiguration);
        services.AddSingleton<IDistributedCache>(sp =>
        {
            var options = sp.GetRequiredService<RedisCacheOptions>();
            return new RedisCache(options);
        });
    }
    //protected abstract void ConfigureProjectSpecificServices(IServiceCollection services);
    protected abstract void ConfigureMassTransit(IBusRegistrationConfigurator busRegistrationConfigurator);
    protected virtual void ConfigureEndpoints(IBusRegistrationContext busRegistrationContext, IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator)
    {
        rabbitMqBusFactoryConfigurator.ConfigureEndpoints(busRegistrationContext);
    }


    public new async Task DisposeAsync()
    {
        await _mssqlContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
        //await _redisContainer.DisposeAsync();
    }
}

// Register the endpoint convention for OrderCreatedMessage
//EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:order-created")); // Ensure Queue Names Match: Ensure that the queue name specified in EndpointConvention.Map<OrderCreatedMessage> matches the one that MassTransit is generating automatically for the consumer.
// If MassTransit is creating a queue named order-created, then map the message to that queue:
//EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:order-created"));

//EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:test")); This will not work.

// Notes
// The issue you're facing with EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:test")); 
// likely stems from the fact that the queue name you're specifying("test") does not match the automatically generated queue name that 
// is being used when cfg.ConfigureEndpoints(context) is called for the OrderCreatedConsumer.Here's a detailed explanation of what's happening:

//Why Does queue:order-created Work?
//EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:order-created")); works because it matches the default convention that MassTransit uses for naming the queue associated with a consumer handling the OrderCreatedMessage.

//If you are using a consumer like OrderCreatedConsumer, then the default name for its queue is something like order-created-consumer(or order-created, depending on your configuration).
//By mapping OrderCreatedMessage to queue:order-created, you're closely matching or aligning with the queue name that MassTransit is expecting, either because it's close enough to the auto-generated name or because you've set the name manually somewhere else.

//Why queue:test Does Not Work
//When you map OrderCreatedMessage to queue:test, MassTransit is being told to send OrderCreatedMessage to a specific queue(test). However, if there is no consumer registered to listen on the test queue, the message won't be handled.

//Queue Mismatch: Since you're configuring endpoints dynamically using ConfigureEndpoints(context), MassTransit is likely creating a queue like order-created-consumer for the OrderCreatedConsumer, but it's expecting OrderCreatedMessage to go to the test queue.This mismatch causes the consumer to not receive the message because it's listening on order-created-consumer, but you're sending the message to queue:test.
//Possible Solutions
//Change the Queue Name in the Consumer: If you want the OrderCreatedConsumer to listen to the test queue, you can explicitly set the queue name when configuring the consumer.

//Example:

//cfg.ReceiveEndpoint("test", e =>
//{
//    e.ConfigureConsumer<OrderCreatedConsumer>(context);
//});
//This will make the OrderCreatedConsumer listen on the test queue, so EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:test")); will now work.

//Ensure Queue Names Match: Ensure that the queue name specified in EndpointConvention.Map<OrderCreatedMessage> matches the one that MassTransit is generating automatically for the consumer.If MassTransit is creating a queue named order-created-consumer, then map the message to that queue:

//EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:order-created-consumer"));
//Explicit Queue for the Message: If you want OrderCreatedMessage to go to a custom queue(test), you need to make sure that either:

//There is a consumer listening on that queue, or
//You manually configure an endpoint for that queue like this:

//cfg.ReceiveEndpoint("test", e =>
//{
//    // Configure the endpoint to consume OrderCreatedMessage
//    e.ConfigureConsumer<OrderCreatedConsumer>(context);
//});

//Conclusion
//The EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:order-created")); works because the queue name aligns with the convention MassTransit uses to name the consumer’s queue.When you change it to queue:test, the consumer isn’t listening on that queue by default. To fix this, you need to either configure the consumer to listen on the test queue or make sure the queue name in EndpointConvention.Map matches the one generated by MassTransit.