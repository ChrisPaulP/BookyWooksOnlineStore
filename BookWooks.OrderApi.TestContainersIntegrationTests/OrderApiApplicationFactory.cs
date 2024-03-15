using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace BookWooks.OrderApi.TestContainersIntegrationTests;

public class OrderApiApplicationFactory<TEntryPoint> : WebApplicationFactory<Program>, IAsyncLifetime where TEntryPoint : Program // : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string Database = "master";
    private const string Username = "sa";
    private const string Password = "yourStrong(!)Password";
    private string _connectionString;
    private const ushort MsSqlPort = 1433;
    private const ushort RabbitMqPort = 5672; 
    private readonly MsSqlContainer _mssqlContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;

    public HttpClient HttpClient { get; private set; } = default!;

    private const string RabbitMqUsername = "guest";
    private const string RabbitMqPassword = "guest";

    public OrderApiApplicationFactory()
    {
        _mssqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            // Remove static port binding and let Docker assign random ports
            .WithPortBinding(0, MsSqlPort)
            .WithEnvironment("ACCEPT_EULA", "Y") 
            .WithEnvironment("SA_PASSWORD", Password)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(MsSqlPort))
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management-alpine") 
            .WithPortBinding(5672, RabbitMqPort) // Bind RabbitMQ's default port to the specified host port
            .WithEnvironment("RABBITMQ_DEFAULT_USER", RabbitMqUsername)
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", RabbitMqPassword)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(RabbitMqPort) // Wait until the specified host port is available
                .UntilPortIsAvailable(15672)) // Wait until the management port is available
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mssqlContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
       _rabbitMqContainer.GetMappedPublicPort(RabbitMqPort);
        GetDatabaseConnectionString();
        var x = _rabbitMqContainer.Hostname;

        HttpClient = CreateClient();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("testcontainersappsettings.json")
       .AddInMemoryCollection(new Dictionary<string, string>
       {
           ["ConnectionStrings:DefaultConnection"] =  _connectionString,
           ["RabbitMQConfiguration:Config:HostName"] = _rabbitMqContainer.Hostname
       })
       .AddEnvironmentVariables()
       .Build();

            configurationBuilder.AddConfiguration(configuration);
        });

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
            options.UseSqlServer(_connectionString,
                builder => builder.MigrationsAssembly(typeof(T).Assembly.FullName));
        });
    }
    private string GetDatabaseConnectionString()
    {
        _connectionString = _mssqlContainer.GetConnectionString(); // $"Server={_mssqlContainer.Hostname},{_mssqlContainer.GetMappedPublicPort(MsSqlPort)};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=True";
        return _connectionString;
    }

    public new async Task DisposeAsync()
    {
        await _mssqlContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
    }
}

