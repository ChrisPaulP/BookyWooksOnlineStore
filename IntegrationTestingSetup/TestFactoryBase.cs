
using Microsoft.AspNetCore.Hosting;



namespace IntegrationTestingSetup;

using BookWooks.OrderApi.Infrastructure.Data;
using BookWooks.OrderApi.Infrastructure.Data.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit; // Needed for IAsyncLifetime (xUnit)


public abstract class TestFactoryBase<TEntryPoint>
    : WebApplicationFactory<TEntryPoint>, IAsyncLifetime where TEntryPoint : class
{
    public readonly MsSqlContainer SqlContainer;
    private readonly RabbitMqContainer RabbitMqContainer;
    private readonly RedisContainer RedisContainer;

    private const string RabbitMqUsername = "guest";
    private const string RabbitMqPassword = "guest";
    private static bool _containersStarted = false;
    private static readonly object _lock = new();

    protected IConfiguration Configuration { get; private set; } = default!;

    protected TestFactoryBase()
    {
        // ✅ SQL Server (Testcontainers)
        SqlContainer = new MsSqlBuilder()
            .WithPassword("Your_password123")
            .Build();

        // ✅ RabbitMQ (specialized container)
        RabbitMqContainer = new RabbitMqBuilder()
            .WithUsername(RabbitMqUsername)
            .WithPassword(RabbitMqPassword)
            .Build();

        // ✅ Redis
        RedisContainer = new RedisBuilder()
            .WithImage("redis:7")
            .WithPortBinding(6379, true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        // ✅ Start containers only once across all test classes (parallel-safe)
        if (!_containersStarted)
        {
            lock (_lock)
            {
                if (!_containersStarted)
                {
                    _containersStarted = true;
                }
            }

            await SqlContainer.StartAsync();
            await RabbitMqContainer.StartAsync();
            await RedisContainer.StartAsync();

            Console.WriteLine($"[DEBUG] SQL running at: {SqlContainer.GetConnectionString()}");
            Console.WriteLine($"[DEBUG] RabbitMQ running at: {RabbitMqContainer.Hostname}:{RabbitMqContainer.GetMappedPublicPort(5672)}");
            Console.WriteLine($"[DEBUG] Redis running at: {RedisContainer.Hostname}:{RedisContainer.GetMappedPublicPort(6379)}");
        }

        // ✅ Ensure database exists (EF migrations expect it)
        await EnsureDatabaseCreatedAsync(SqlContainer.GetConnectionString());

        //// ✅ Auto-seed DB once per test suite run (optional)
        //using var scope = Services.CreateScope();
        //var dbContext = scope.ServiceProvider.GetRequiredService<BookyWooksOrderDbContext>();
        //await dbContext.Database.MigrateAsync();
        //await DatabaseExtentions.ClearData(dbContext);
        //await DatabaseExtentions.SeedAsync(dbContext);
        //Console.WriteLine($"[DEBUG] EF Core seeded using connection: {dbContext.Database.GetConnectionString()}");
    }

    private async Task EnsureDatabaseCreatedAsync(string masterConnection)
    {
        var builder = new SqlConnectionStringBuilder(masterConnection)
        {
            InitialCatalog = "BookyWooksOrderDbContext"
        };

        var databaseConnectionString = builder.ConnectionString;

        using var masterSqlConnection = new SqlConnection(masterConnection);
        await masterSqlConnection.OpenAsync();

        var dbName = builder.InitialCatalog;
        using var cmd = masterSqlConnection.CreateCommand();
        cmd.CommandText = $@"
            IF DB_ID('{dbName}') IS NULL
            BEGIN
                CREATE DATABASE [{dbName}];
            END";
        await cmd.ExecuteNonQueryAsync();

        Console.WriteLine($"[DEBUG] Created test database: {dbName}");

        // ✅ Replace SqlContainer.GetConnectionString() with our new DB name
        _testDatabaseConnectionString = databaseConnectionString;
    }

    private string _testDatabaseConnectionString = default!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(_ =>
        {
            var sqlConn = _testDatabaseConnectionString ?? SqlContainer.GetConnectionString();
            var rabbitPort = RabbitMqContainer.GetMappedPublicPort(5672);
            var redisConn = $"{RedisContainer.Hostname}:{RedisContainer.GetMappedPublicPort(6379)}";

            Console.WriteLine($"[DEBUG] SQL connection: {sqlConn}");
            Console.WriteLine($"[DEBUG] RabbitMQ connection: {RabbitMqContainer.Hostname}:{rabbitPort}");
            Console.WriteLine($"[DEBUG] Redis connection: {redisConn}");

            Configuration = new ConfigurationBuilder()
                .AddJsonFile("testcontainersappsettings.json", optional: false)
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = sqlConn,
                    ["ConnectionStrings:OrderDatabase"] = sqlConn,
                    ["ConnectionStrings:SagaOrchestrationDatabase"] = sqlConn,
                    ["ConnectionStrings:Redis"] = redisConn,

                    ["RabbitMQConfiguration:Config:HostName"] = RabbitMqContainer.Hostname,
                    ["RabbitMQConfiguration:Config:Port"] = rabbitPort.ToString(),
                    ["RabbitMQConfiguration:Config:UserName"] = RabbitMqUsername,
                    ["RabbitMQConfiguration:Config:Password"] = RabbitMqPassword
                })
                .Build();
        });

        builder.ConfigureTestServices(services =>
        {
            // ✅ Override EF Core DbContext to use Testcontainers SQL connection
            services.AddDbContext<BookyWooksOrderDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // ✅ Override MassTransit (RabbitMQ)
            services.AddMassTransitTestHarness(cfg =>
            {
                ConfigureMassTransit(cfg);

                cfg.UsingRabbitMq((context, rabbitCfg) =>
                {
                    var host = RabbitMqContainer.Hostname;
                    var port = RabbitMqContainer.GetMappedPublicPort(5672);

                    rabbitCfg.Host(host, port, "/", h =>
                    {
                        h.Username(RabbitMqUsername);
                        h.Password(RabbitMqPassword);
                    });

                    ConfigureEndpoints(context, rabbitCfg);
                });
            });

            // ✅ Optionally override Redis (if needed)
            //services.AddStackExchangeRedisCache(options =>
            //{
            //    options.Configuration = Configuration.GetConnectionString("Redis");
            //});
        });
    }

    public Task DisposeAsync()
    {
        // We do not dispose containers after each test class to speed up test runs
        return Task.CompletedTask;
    }

    protected abstract void ConfigureMassTransit(IBusRegistrationConfigurator cfg);

    protected virtual void ConfigureEndpoints(IBusRegistrationContext ctx, IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.ConfigureEndpoints(ctx);
    }
}
//public abstract class TestFactoryBase<TEntryPoint>
//    : WebApplicationFactory<TEntryPoint>, IAsyncLifetime where TEntryPoint : class
//{
//    // ✅ Shared static containers (parallel tests across classes use same instances)
//    //public static readonly MsSqlContainer SqlContainer = IntegrationTestingSetupExtensions.CreateMsSqlContainer();
//    //private static readonly RabbitMqContainer RabbitMqContainer = IntegrationTestingSetupExtensions.CreateRabbitMqContainer();
//    //private static readonly RedisContainer RedisContainer = IntegrationTestingSetupExtensions.CreateRedisContainer();

//    public readonly MsSqlContainer SqlContainer;
//    private readonly RabbitMqContainer RabbitMqContainer;
//    private readonly RedisContainer RedisContainer;

//    private const string RabbitMqUsername = "guest";
//    private const string RabbitMqPassword = "guest";
//    private static bool _containersStarted = false;

//    protected IConfiguration Configuration { get; private set; } = default!;

//    protected TestFactoryBase()
//    {
//        // ✅ SQL Server
//        SqlContainer = new MsSqlBuilder()
//            .WithPassword("Your_password123")
//            .Build();

//        // ✅ RabbitMQ (specialized container)
//        RabbitMqContainer = new RabbitMqBuilder()
//            .WithUsername("guest")
//            .WithPassword("guest")
//            .Build();

//        // ✅ Redis (no specialized container, so use generic)
//        RedisContainer = new RedisBuilder()
//            .WithImage("redis:7")
//            .WithPortBinding(6379, true)
//            .Build();
//    }

//    public async Task InitializeAsync()
//    {
//        // ✅ Start containers only once across all test classes (parallel-safe)
//        //if (!_containersStarted)
//        //{
//            await SqlContainer.StartAsync();
//            await RabbitMqContainer.StartAsync();
//            await RedisContainer.StartAsync();
//            //_containersStarted = true;

//            Console.WriteLine($"[DEBUG] SQL running at: {SqlContainer.GetConnectionString()}");
//            Console.WriteLine($"[DEBUG] RabbitMQ running at: {RabbitMqContainer.Hostname}:{RabbitMqContainer.GetMappedPublicPort(5672)}");
//            Console.WriteLine($"[DEBUG] Redis running at: {RedisContainer.Hostname}:{RedisContainer.GetMappedPublicPort(6379)}");
//        //}

//        // ✅ Auto-seed DB once per test suite run (fast in CI)
//        //using var scope = Services.CreateScope();
//        //var dbContext = scope.ServiceProvider.GetRequiredService<BookyWooksOrderDbContext>();

//        //await dbContext.Database.MigrateAsync();
//        //await DatabaseExtentions.ClearData(dbContext);
//        //await DatabaseExtentions.SeedAsync(dbContext);

//        //Console.WriteLine($"[DEBUG] EF Core seeded using connection: {dbContext.Database.GetConnectionString()}");
//    }

//    protected override void ConfigureWebHost(IWebHostBuilder builder)
//    {
//        builder.ConfigureAppConfiguration(_ =>
//        {
//            var sqlConn = SqlContainer.GetConnectionString();
//            var rabbitPort = RabbitMqContainer.GetMappedPublicPort(5672);
//            var redisConn = $"{RedisContainer.Hostname}:{RedisContainer.GetMappedPublicPort(6379)}";

//            Console.WriteLine($"[DEBUG] SQL connection: {sqlConn}");
//            Console.WriteLine($"[DEBUG] RabbitMQ connection: {RabbitMqContainer.Hostname}:{rabbitPort}");
//            Console.WriteLine($"[DEBUG] Redis connection: {redisConn}");

//            Configuration = new ConfigurationBuilder()
//                .AddJsonFile("testcontainersappsettings.json", optional: false) // <-- set optional: false
//                //.AddEnvironmentVariables()
//                .AddInMemoryCollection(new Dictionary<string, string>
//                {
//                    ["ConnectionStrings:DefaultConnection"] = sqlConn,
//                    ["ConnectionStrings:TestConnection"] = sqlConn,
//                    ["ConnectionStrings:OrderDatabase"] = sqlConn,
//                    ["ConnectionStrings:SagaOrchestrationDatabase"] = sqlConn,
//                    ["RabbitMQConfiguration:Config:HostName"] = RabbitMqContainer.Hostname,
//                    ["RabbitMQConfiguration:Config:Port"] = rabbitPort.ToString(),
//                    ["RabbitMQConfiguration:Config:UserName"] = RabbitMqUsername,
//                    ["RabbitMQConfiguration:Config:Password"] = RabbitMqPassword,
//                    ["ConnectionStrings:Redis"] = redisConn
//                })
//                .Build();
//                    });

//        builder.ConfigureTestServices(services =>
//        {
//            // ✅ Override DbContext with Testcontainers connection
//            //OverrideDbContext(services, Configuration);

//            //// ✅ Override Redis
//            //OverrideRedis(services, Configuration);

//            // ✅ Override MassTransit (RabbitMQ)
//            services.AddMassTransitTestHarness(cfg =>
//            {
//                ConfigureMassTransit(cfg);

//                cfg.UsingRabbitMq((context, rabbitCfg) =>
//                {
//                    var host = RabbitMqContainer.Hostname;
//                    var port = RabbitMqContainer.GetMappedPublicPort(5672);

//                    rabbitCfg.Host(host, port, "/", h =>
//                    {
//                        h.Username(RabbitMqUsername);
//                        h.Password(RabbitMqPassword);
//                    });

//                    ConfigureEndpoints(context, rabbitCfg);
//                });
//            });
//        });
//    }

//    private static void OverrideDbContext(IServiceCollection services, IConfiguration configuration)
//    {
//        var descriptors = services
//            .Where(s => s.ServiceType == typeof(DbContextOptions<BookyWooksOrderDbContext>))
//            .ToList();

//        foreach (var d in descriptors)
//            services.Remove(d);

//        services.AddDbContext<BookyWooksOrderDbContext>(options =>
//            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
//    }

//    private static void OverrideRedis(IServiceCollection services, IConfiguration configuration)
//    {
//        var redisConn = configuration.GetValue<string>("ConnectionStrings:Redis")
//            ?? throw new ArgumentNullException("Redis connection string not configured");

//        Console.WriteLine($"[DEBUG] Overriding Redis with: {redisConn}");

//        // Remove existing registrations
//        var descriptors = services
//            .Where(s => s.ServiceType == typeof(IDistributedCache) || s.ServiceType == typeof(RedisCacheOptions))
//            .ToList();
//        foreach (var d in descriptors)
//            services.Remove(d);

//        // Re-register
//        var redisConfiguration = new RedisCacheOptions
//        {
//            ConfigurationOptions = new ConfigurationOptions
//            {
//                AbortOnConnectFail = true,
//                EndPoints = { redisConn }
//            }
//        };

//        services.AddSingleton(redisConfiguration);
//        services.AddSingleton<IDistributedCache>(_ => new RedisCache(redisConfiguration));
//    }

//    protected abstract void ConfigureMassTransit(IBusRegistrationConfigurator cfg);

//    protected virtual void ConfigureEndpoints(IBusRegistrationContext ctx, IRabbitMqBusFactoryConfigurator cfg)
//    {
//        cfg.ConfigureEndpoints(ctx);
//    }

//    public new Task DisposeAsync()
//    {
//        // ✅ DO NOT stop containers – keep them alive for entire test run
//        return Task.CompletedTask;
//    }
//}



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