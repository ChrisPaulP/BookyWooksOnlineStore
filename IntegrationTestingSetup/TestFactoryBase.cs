namespace IntegrationTestingSetup;
public abstract class TestFactoryBase<TEntryPoint>: WebApplicationFactory<TEntryPoint>, IAsyncLifetime where TEntryPoint : class
{
    public readonly MsSqlContainer SqlContainer;
    private readonly RabbitMqContainer RabbitMqContainer;
    private readonly RedisContainer RedisContainer;

    private const string RabbitMqUsername = "guest";
    private const string RabbitMqPassword = "guest";
    private static bool _containersStarted = false;
    private static readonly object _lock = new();

    private string _testDatabaseConnectionString = default!;
    protected IConfiguration Configuration { get; private set; } = default!;

    public string RabbitMqHost => RabbitMqContainer.Hostname;
    public ushort RabbitMqPort => RabbitMqContainer.GetMappedPublicPort(5672);
    public ushort RedisPort => RedisContainer.GetMappedPublicPort(6379);
    protected TestFactoryBase()
    {
        SqlContainer = new MsSqlBuilder()
            .WithPassword("Your_password123")
            .Build();

        RabbitMqContainer = new RabbitMqBuilder()
            .WithUsername(RabbitMqUsername)
            .WithPassword(RabbitMqPassword)
            .Build();

        RedisContainer = new RedisBuilder()
            .WithImage("redis:7")
            .WithPortBinding(6379, true)
            .Build();
    }

    public async Task InitializeAsync()
    {
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

            Console.WriteLine($"[DEBUG] SQL: {SqlContainer.GetConnectionString()}");
            Console.WriteLine($"[DEBUG] RabbitMQ: {RabbitMqHost}:{RabbitMqPort}");
            Console.WriteLine($"[DEBUG] Redis: {RedisContainer.Hostname}:{RedisPort}");
        }

        await EnsureDatabaseCreatedAsync(SqlContainer.GetConnectionString());

    }

    private async Task EnsureDatabaseCreatedAsync(string masterConnection)
    {
        var builder = new SqlConnectionStringBuilder(masterConnection)
        {
            InitialCatalog = "BookyWooksOrderDbContext"
        };

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

        _testDatabaseConnectionString = builder.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(_ =>
        {
            var sqlConn = _testDatabaseConnectionString ?? SqlContainer.GetConnectionString();
            var rabbitPort = RabbitMqPort;
            var redisConn = $"{RedisContainer.Hostname}:{RedisContainer.GetMappedPublicPort(6379)}";

            Configuration = new ConfigurationBuilder()
                .AddJsonFile("testcontainersappsettings.json", optional: false)
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:OrderDatabase"] = sqlConn,
                    ["ConnectionStrings:Redis"] = redisConn,
                    ["RabbitMQConfiguration:Config:HostName"] = RabbitMqHost,
                    ["RabbitMQConfiguration:Config:Port"] = rabbitPort.ToString(),
                    ["RabbitMQConfiguration:Config:UserName"] = RabbitMqUsername,
                    ["RabbitMQConfiguration:Config:Password"] = RabbitMqPassword
                })
                .Build();
        });

        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<BookyWooksOrderDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<BookyWooksOrderDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("OrderDatabase")));

            // ✅ Replace MassTransit with Test Harness
            services.AddMassTransitTestHarness(cfg =>
            {
                ConfigureMassTransit(cfg);

                cfg.UsingRabbitMq((context, rabbitCfg) =>
                {
                    rabbitCfg.Host(RabbitMqHost, RabbitMqPort, "/", h =>
                    {
                        h.Username(RabbitMqUsername);
                        h.Password(RabbitMqPassword);
                    });

                    ConfigureEndpoints(context, rabbitCfg);
                });
            });
        });
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected abstract void ConfigureMassTransit(IBusRegistrationConfigurator cfg);

    protected virtual void ConfigureEndpoints(IBusRegistrationContext ctx, IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.ConfigureEndpoints(ctx);
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