using Docker.DotNet.Models;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Net.Http.Headers;

namespace BookWooks.OrderApi.TestContainersIntegrationTests.TestSetup;

public abstract class TestFactoryBase<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
    where TEntryPoint : class
{
    private readonly INetwork _network;
    private readonly TestContainerBuilder _containerBuilder;
    private static readonly object _lock = new();
    private static bool _containersStarted;
    private string _testDatabaseConnectionString = default!;

    public readonly MsSqlContainer SqlContainer;
    private readonly RabbitMqContainer RabbitMqContainer;
    private readonly RedisContainer RedisContainer;
    private QdrantContainer QdrantContainer;
    private IContainer McpServerContainer;
    protected IConfiguration Configuration { get; private set; } = default!;

    public string RabbitMqHost => RabbitMqContainer.Hostname;
    public ushort RabbitMqPort => RabbitMqContainer.GetMappedPublicPort(5672);
    public ushort RedisPort => RedisContainer.GetMappedPublicPort(6379);
    public string QdrantHost => QdrantContainer.Hostname;
    public ushort QdrantPort => QdrantContainer.GetMappedPublicPort(6334);

    protected TestFactoryBase()
    {
        try
        {
            var timestamp = DateTime.UtcNow.Ticks;
            _network = new NetworkBuilder()
                .WithName($"integration-tests")
                .Build();

            _containerBuilder = new TestContainerBuilder(_network);

            // Initialize containers
            SqlContainer = _containerBuilder.BuildSqlContainer();
            RabbitMqContainer = _containerBuilder.BuildRabbitMqContainer();
            RedisContainer = _containerBuilder.BuildRedisContainer();
            QdrantContainer = _containerBuilder.BuildQdrantContainer();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to initialize containers: {ex.Message}");
            throw;
        }
    }

    public async Task InitializeAsync()
    {
        if (ShouldInitializeContainers())
        {
            await InitializeContainersAsync();
            await InitializeDatabaseAsync();
            await InitializeVectorEmbeddingsAsync();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(ConfigureAppSettings);
        builder.ConfigureTestServices(ConfigureTestServices);
    }

    private void ConfigureAppSettings(IConfigurationBuilder configBuilder)
    {
        Environment.SetEnvironmentVariable("MCPSERVER__HOST", McpServerContainer.Hostname);
        Environment.SetEnvironmentVariable("MCPSERVER__PORT", McpServerContainer.GetMappedPublicPort(8181).ToString());
        // Add Qdrant configuration to the environment
        Environment.SetEnvironmentVariable("QdrantOptions__QdrantHost", QdrantContainer.Hostname);
        Environment.SetEnvironmentVariable("QdrantOptions__QdrantPort", QdrantContainer.GetMappedPublicPort(6334).ToString());
        Environment.SetEnvironmentVariable("QdrantOptions__ApiKey", "your-secret-api-key-here");

        var connectionStrings = new Dictionary<string, string>
        {
            ["ConnectionStrings:OrderDatabase"] = _testDatabaseConnectionString ?? SqlContainer.GetConnectionString(),
            ["ConnectionStrings:Redis"] = $"{RedisContainer.Hostname}:{RedisPort}",
            ["RabbitMQConfiguration:Config:HostName"] = RabbitMqHost,
            ["RabbitMQConfiguration:Config:Port"] = RabbitMqPort.ToString(),
            ["RabbitMQConfiguration:Config:UserName"] = ContainerConfiguration.RabbitMqUsername,
            ["RabbitMQConfiguration:Config:Password"] = ContainerConfiguration.RabbitMqPassword,
            ["McpServer:MCPSERVER__HOST"] = McpServerContainer.Hostname,
            ["McpServer:MCPSERVER__PORT"] = McpServerContainer.GetMappedPublicPort(8181).ToString(),
            ["QdrantOptions:QdrantHost"] = QdrantContainer.Hostname,
            ["QdrantOptions:QdrantPort"] = "6334",
            ["QdrantOptions:ApiKey"] = "your-secret-api-key-here"
        };

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(connectionStrings)
            .Build();
    }

    private void ConfigureTestServices(IServiceCollection services)
    {
        TestServiceConfiguration.ConfigureServices(services, Configuration, QdrantContainer);
        ConfigureMassTransitTestHarness(services);
    }

    private void ConfigureMassTransitTestHarness(IServiceCollection services)
    {
        services.AddMassTransitTestHarness(cfg =>
        {
            ConfigureMassTransit(cfg);

            cfg.UsingRabbitMq((context, rabbitCfg) =>
            {
                rabbitCfg.Host(RabbitMqHost, RabbitMqPort, "/", h =>
                {
                    h.Username(ContainerConfiguration.RabbitMqUsername);
                    h.Password(ContainerConfiguration.RabbitMqPassword);
                });

                ConfigureEndpoints(context, rabbitCfg);
            });
        });
    }

    private bool ShouldInitializeContainers()
    {
        lock (_lock)
        {
            if (_containersStarted) return false;
            _containersStarted = true;
            return true;
        }
    }

    private async Task InitializeContainersAsync()
    {
        try
        {
            await _network.CreateAsync();
            await SqlContainer.StartAsync();
            await QdrantContainer.StartAsync();

            McpServerContainer = _containerBuilder.BuildMcpServerContainer(QdrantContainer);
            await McpServerContainer.StartAsync();

            await Task.WhenAll(
                RabbitMqContainer.StartAsync(),
                RedisContainer.StartAsync()
            );

           // await WaitForMcpServerReadiness();
        }
        catch (DockerApiException ex) when (ex.Message.Contains("already exists"))
        {
            Console.WriteLine("[DEBUG] Using existing Docker network");
        }
    }

    private async Task InitializeDatabaseAsync()
    {
        await EnsureDatabaseCreatedAsync(SqlContainer.GetConnectionString());
    }

    private async Task InitializeVectorEmbeddingsAsync()
    {
        using var scope = Services.CreateScope();
        try
        {
            var syncService = scope.ServiceProvider.GetRequiredService<ProductEmbeddingSyncService>();
            await syncService.PopulateAsync("products");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to initialize vector embeddings: {ex.Message}");
            throw;
        }
    }

    protected virtual void ConfigureEndpoints(IBusRegistrationContext ctx, IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.ConfigureEndpoints(ctx);
    }

    protected abstract void ConfigureMassTransit(IBusRegistrationConfigurator cfg);

    private async Task EnsureDatabaseCreatedAsync(string masterConnection)
    {
        var maxRetries = 30;
        var retryDelay = TimeSpan.FromSeconds(1);
        var builder = new SqlConnectionStringBuilder(masterConnection)
        {
            InitialCatalog = "BookyWooksOrderDbContext"
        };

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
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
                Console.WriteLine("[DEBUG] Database initialization successful");
                return;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[DEBUG] Database connection attempt {i + 1}/{maxRetries} failed: {ex.Message}");
                if (i < maxRetries - 1)
                {
                    await Task.Delay(retryDelay);
                }
                else
                {
                    throw; // Re-throw if we've exhausted all retries
                }
            }
        }
    }
    public async Task DisposeAsync()
    {
        
            await Task.WhenAll(
                SqlContainer.DisposeAsync().AsTask(),
                RabbitMqContainer.DisposeAsync().AsTask(),
                RedisContainer.DisposeAsync().AsTask(),
                QdrantContainer.DisposeAsync().AsTask(),
                McpServerContainer.DisposeAsync().AsTask()
            );
            await _network.DisposeAsync();
        
    }
}
