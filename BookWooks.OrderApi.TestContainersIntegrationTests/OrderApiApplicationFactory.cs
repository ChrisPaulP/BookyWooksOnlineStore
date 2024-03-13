using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;

namespace BookWooks.OrderApi.TestContainersIntegrationTests;

public class OrderApiApplicationFactory<TEntryPoint> : WebApplicationFactory<Program>, IAsyncLifetime where TEntryPoint : Program // : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string Database = "master";
    private const string Username = "sa";
    private const string Password = "yourStrong(!)Password";
    private string _connectionString;
    private const ushort MsSqlPort = 1433;
    private const ushort RabbitMqPort = 5672; // RabbitMQ default port
    private readonly MsSqlContainer _mssqlContainer;
    private readonly IContainer _rabbitMqContainer;

    public HttpClient HttpClient { get; private set; } = default!;

    private const string RabbitMqUsername = "guest";
    private const string RabbitMqPassword = "guest";

    public OrderApiApplicationFactory()
    {
        _mssqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPortBinding(5433, 1433) 
            .WithEnvironment("ACCEPT_EULA", "Y") 
            .WithEnvironment("SA_PASSWORD", Password)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(MsSqlPort))
            .Build();

        _rabbitMqContainer = new ContainerBuilder()
            .WithImage("rabbitmq:3-management-alpine") // Use rabbitmq:management image
            .WithPortBinding(RabbitMqPort, 5672)
            .WithPortBinding(15672, 15672) // Management plugin
            .WithEnvironment("RabbitMQConfiguration__Config__UserName", RabbitMqUsername)
            .WithEnvironment("RabbitMQConfiguration__Config__Password", RabbitMqPassword)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(15672)) // Adding wait strategy
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mssqlContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
       
        GetDatabaseConnectionString(); //$"Server={host},{port};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=True";

        //_respawner = await CreateRespawnerAsync();
        HttpClient = CreateClient();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            //var configuration = new ConfigurationBuilder()
            //   .AddJsonFile("testcontainersappsettings.json")
            //   .AddEnvironmentVariables()
            //   .Build();
            //string connectionString = configuration.GetConnectionString("TestConnection");

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
        _connectionString = $"Server={_mssqlContainer.Hostname},{_mssqlContainer.GetMappedPublicPort(MsSqlPort)};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=True";
        return _connectionString;
    }

    public new async Task DisposeAsync()
    {
        await _mssqlContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
    }
}

