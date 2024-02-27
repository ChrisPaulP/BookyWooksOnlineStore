namespace BookWooks.OrderApi.TestContainersIntegrationTests;

public class OrderApiApplicationFactory<TEntryPoint> : WebApplicationFactory<Program>, IAsyncLifetime where TEntryPoint : Program // : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string Database = "master";
    private const string Username = "sa";
    private const string Password = "yourStrong(!)Password";
    private string ConnectionString;
    private const ushort MsSqlPort = 1433;
    private const ushort RabbitMqPort = 5672; // RabbitMQ default port
    private readonly IContainer _mssqlContainer;
    private readonly IContainer _rabbitMqContainer;

    public HttpClient HttpClient { get; private set; } = default!;

    private const string RabbitMqUsername = "guest";
    private const string RabbitMqPassword = "guest";

    public OrderApiApplicationFactory()
    {
        _mssqlContainer = new ContainerBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPortBinding(MsSqlPort, true)
            .WithEnvironment("ACCEPT_EULA", "Y") 
            .WithEnvironment("SQLCMDUSER", Username)
            .WithEnvironment("SQLCMDPASSWORD", Password)
            .WithEnvironment("MSSQL_SA_PASSWORD", Password)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(MsSqlPort))
            .Build();

        _rabbitMqContainer = new ContainerBuilder()
            .WithImage("rabbitmq:3-management-alpine") // Use rabbitmq:management image
            .WithPortBinding(RabbitMqPort, 5672)
            .WithEnvironment("RABBITMQ_DEFAULT_USER", RabbitMqUsername)
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", RabbitMqPassword)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(RabbitMqPort)) // Adding wait strategy
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mssqlContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        ConnectionString = GetDatabaseConnectionString(); //$"Server={host},{port};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=True";

        //_respawner = await CreateRespawnerAsync();
        HttpClient = CreateClient();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            RemoveDbContextOptions<BookyWooksOrderDbContext>(services);
            RemoveDbContextOptions<IntegrationEventLogDbContext>(services);

            AddDbContext<BookyWooksOrderDbContext>(services);
            AddDbContext<IntegrationEventLogDbContext>(services);
        });
    }

    private void RemoveDbContextOptions<T>(IServiceCollection services) where T : DbContext
    {
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<T>));

        if (descriptor != null)
            services.Remove(descriptor);
    }

    private void AddDbContext<T>(IServiceCollection services) where T : DbContext
    {
        services.AddDbContext<T>(options =>
        {
            options.UseSqlServer(ConnectionString,
                builder => builder.MigrationsAssembly(typeof(T).Assembly.FullName));
        });
    }
    private string GetDatabaseConnectionString()
    {
        return $"Server={_mssqlContainer.Hostname},{_mssqlContainer.GetMappedPublicPort(MsSqlPort)};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=True";
    }

    public new async Task DisposeAsync()
    {
        await _mssqlContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
    }
}

