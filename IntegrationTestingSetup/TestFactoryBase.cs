using BookWooks.OrderApi.Infrastructure; // Your EF DbContext namespace
using BookWooks.OrderApi.Infrastructure.Data;
using DotNet.Testcontainers.Containers;
using IntegrationTestingSetup;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

public abstract class TestFactoryBase<TEntryPoint> :
    WebApplicationFactory<TEntryPoint>, IAsyncLifetime
    where TEntryPoint : class
{
    protected readonly MsSqlContainer _mssqlContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;
    private const string TestDatabaseName = "BookyWooksTest";
    private const string RabbitMqUsername = "guest";
    private const string RabbitMqPassword = "guest";
    protected IConfiguration Configuration { get; private set; }

    protected TestFactoryBase()
    {
        _mssqlContainer = IntegrationTestingSetupExtensions.CreateMsSqlContainer();
        _rabbitMqContainer = IntegrationTestingSetupExtensions.CreateRabbitMqContainer();
    }

    public async Task InitializeAsync()
    {
        // ✅ Start containers BEFORE WebHost starts
        await _mssqlContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        Console.WriteLine($"[DEBUG] SQL Connection: {_mssqlContainer.GetConnectionString()}");

        // ✅ Ensure a clean test database exists
        await EnsureTestDatabaseAsync(_mssqlContainer.GetConnectionString(), TestDatabaseName);
    }

    private static async Task EnsureTestDatabaseAsync(string masterConnectionString, string databaseName)
    {
        await using var connection = new  SqlConnection(masterConnectionString);
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $@"
            IF DB_ID('{databaseName}') IS NULL
            BEGIN
                CREATE DATABASE [{databaseName}];
            END";
        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine($"[DEBUG] Ensured test database: {databaseName}");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var testDbConnectionString =
            $"{_mssqlContainer.GetConnectionString()};Database={TestDatabaseName}";

        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var testConfig = new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = _mssqlContainer.GetConnectionString(),
                ["ConnectionStrings:SagaOrchestrationDatabase"] = _mssqlContainer.GetConnectionString(),
                ["RabbitMQConfiguration:Config:HostName"] = _rabbitMqContainer.Hostname,

                // ✅ Disable OpenTelemetry for integration tests
                ["OpenTelemetry:TracingEnabled"] = "false",
                ["OpenTelemetry:MetricsEnabled"] = "false",

                ["OpenAIOptions:DisabledForIntegrationTests"] = "true"
            };

            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfig)
                .AddEnvironmentVariables() // still allow overrides from CI if needed
                .Build();
        });

        builder.ConfigureTestServices(services =>
        {
            // ✅ Force EF Core to use the Testcontainers database
            services.RemoveAll<DbContextOptions<BookyWooksOrderDbContext>>();
            services.AddDbContext<BookyWooksOrderDbContext>(options =>
            {
                Console.WriteLine($"[DEBUG] FORCED EF ConnectionString: {testDbConnectionString}");
                options.UseSqlServer(testDbConnectionString);
            });
            // ✅ Register No-Op MeterProvider for tests
            services.RemoveAll<MeterProvider>();
            services.AddSingleton(sp =>
                Sdk.CreateMeterProviderBuilder().Build());

            // ✅ Configure MassTransit Test Harness
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
        });
    }

    protected abstract void ConfigureMassTransit(IBusRegistrationConfigurator busRegistrationConfigurator);

    protected virtual void ConfigureEndpoints(
        IBusRegistrationContext busRegistrationContext,
        IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator)
    {
        rabbitMqBusFactoryConfigurator.ConfigureEndpoints(busRegistrationContext);
    }

    public new async Task DisposeAsync()
    {
        await _mssqlContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
    }
}
