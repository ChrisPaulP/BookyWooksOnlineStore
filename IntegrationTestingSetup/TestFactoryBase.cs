
using Microsoft.AspNetCore.Hosting;



namespace IntegrationTestingSetup;

using System;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using Xunit; // Needed for IAsyncLifetime (xUnit)
using MassTransit;
using Microsoft.AspNetCore.TestHost;

using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;



public abstract class TestFactoryBase<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
    where TEntryPoint : class
{
    private static readonly Lazy<Task> _containerInit = new(() => InitializeContainersAsync());
    private static bool _containersInitialized;

    protected static MsSqlContainer SqlContainer { get; private set; }
    protected static RabbitMqContainer RabbitMqContainer { get; private set; }
    protected static RedisContainer RedisContainer { get; private set; }

    private const string RabbitMqUsername = "guest";
    private const string RabbitMqPassword = "guest";

    protected IConfiguration Configuration { get; private set; }

    public async Task InitializeAsync()
    {
        if (!_containersInitialized)
        {
            await _containerInit.Value;
            _containersInitialized = true;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var sqlConn = SqlContainer.GetConnectionString();
            var redisConn = $"{RedisContainer.Hostname}:{RedisContainer.GetMappedPublicPort(6379)}";
            var rabbitPort = RabbitMqContainer.GetMappedPublicPort(5672);

            Console.WriteLine($"[DEBUG] SQL connection: {sqlConn}");
            Console.WriteLine($"[DEBUG] RabbitMQ connection: {RabbitMqContainer.Hostname}:{rabbitPort}");
            Console.WriteLine($"[DEBUG] Redis connection: {redisConn}");

            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = sqlConn,
                    ["ConnectionStrings:OrderDatabase"] = sqlConn,
                    ["ConnectionStrings:SagaOrchestrationDatabase"] = sqlConn,
                    ["RabbitMQConfiguration:Config:HostName"] = RabbitMqContainer.Hostname,
                    ["RabbitMQConfiguration:Config:Port"] = rabbitPort.ToString(),
                    ["RabbitMQConfiguration:Config:UserName"] = RabbitMqUsername,
                    ["RabbitMQConfiguration:Config:Password"] = RabbitMqPassword,
                    ["ConnectionStrings:Redis"] = redisConn
                })
                //.AddEnvironmentVariables()
                .Build();

            configurationBuilder.AddConfiguration(Configuration);
        });

        builder.ConfigureTestServices(services =>
        {
            OverrideRedis(services, Configuration);

            services.AddMassTransitTestHarness(busRegistrationConfigurator =>
            {
                ConfigureMassTransit(busRegistrationConfigurator);

                busRegistrationConfigurator.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(
                        RabbitMqContainer.Hostname,
                        RabbitMqContainer.GetMappedPublicPort(5672),
                        "/",
                        h =>
                        {
                            h.Username(RabbitMqUsername);
                            h.Password(RabbitMqPassword);
                        });

                    ConfigureEndpoints(context, cfg);
                });
            });
        });
    }

    private static void OverrideRedis(IServiceCollection services, IConfiguration configuration)
    {
        var descriptors = services
            .Where(s => s.ServiceType == typeof(IDistributedCache) || s.ServiceType == typeof(RedisCacheOptions))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }

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

    protected abstract void ConfigureMassTransit(IBusRegistrationConfigurator busRegistrationConfigurator);

    protected virtual void ConfigureEndpoints(
        IBusRegistrationContext busRegistrationContext,
        IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator)
    {
        rabbitMqBusFactoryConfigurator.ConfigureEndpoints(busRegistrationContext);
    }

    public async Task DisposeAsync()
    {
        if (_containersInitialized)
        {
            await SqlContainer.DisposeAsync();
            await RabbitMqContainer.DisposeAsync();
            await RedisContainer.DisposeAsync();
        }
    }

    // ✅ Containers now fully created by your updated IntegrationTestingSetupExtensions
    private static async Task InitializeContainersAsync()
    {
        Console.WriteLine("[DEBUG] Starting Testcontainers via IntegrationTestingSetupExtensions...");

        SqlContainer = IntegrationTestingSetupExtensions.CreateMsSqlContainer();
        RabbitMqContainer = IntegrationTestingSetupExtensions.CreateRabbitMqContainer();
        RedisContainer = IntegrationTestingSetupExtensions.CreateRedisContainer();

        await SqlContainer.StartContainersAsync(RabbitMqContainer, RedisContainer);

        Console.WriteLine($"[DEBUG] SQL running at: {SqlContainer.GetConnectionString()}");
        Console.WriteLine($"[DEBUG] RabbitMQ running at: {RabbitMqContainer.Hostname}:{RabbitMqContainer.GetMappedPublicPort(5672)}");
        Console.WriteLine($"[DEBUG] Redis running at: {RedisContainer.Hostname}:{RedisContainer.GetMappedPublicPort(6379)}");
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