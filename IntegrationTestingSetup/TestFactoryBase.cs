using BookWooks.OrderApi.Infrastructure.Data;
using BookWooks.OrderApi.Infrastructure.Data.Extensions;
using IntegrationTestingSetup;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

public abstract class TestFactoryBase<TEntryPoint>
    : WebApplicationFactory<TEntryPoint>, IAsyncLifetime where TEntryPoint : class
{
    protected readonly MsSqlContainer _mssqlContainer;
    protected readonly RabbitMqContainer _rabbitMqContainer;
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
        await _mssqlContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        // [COPILOT] Wait for RabbitMQ to be fully ready
        Console.WriteLine($"[COPILOT][RabbitMQ] Waiting for RabbitMQ container to be fully ready...");
        await Task.Delay(5000);

        // Optional: Create test database if not exists
        await using var connection = new Microsoft.Data.SqlClient.SqlConnection(_mssqlContainer.GetConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "IF DB_ID('BookyWooksTest') IS NULL CREATE DATABASE [BookyWooksTest];";
        await command.ExecuteNonQueryAsync();

        // ✅ Run migrations & seed the database (same logic as in real app)
        using var scope = Services.CreateScope();
        var appServices = scope.ServiceProvider;
        var context = appServices.GetRequiredService<BookyWooksOrderDbContext>();

        await context.Database.MigrateAsync();
        await DatabaseExtentions.ClearData(context); // you can call the private methods if you make them internal
        await DatabaseExtentions.SeedAsync(context);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var rabbitMqHost = _rabbitMqContainer.Hostname; // [COPILOT]
            var rabbitMqPort = _rabbitMqContainer.GetMappedPublicPort(5672); // [COPILOT] 
            var rabbitMqConnectionString = $"amqp://{RabbitMqUsername}:{RabbitMqPassword}@{rabbitMqHost}:{rabbitMqPort}/"; // [COPILOT]

            Console.WriteLine($"[COPILOT][RabbitMQ] Host: {rabbitMqHost}");
            Console.WriteLine($"[COPILOT][RabbitMQ] Port: {rabbitMqPort}");
            Console.WriteLine($"[COPILOT][RabbitMQ] ConnectionString: {rabbitMqConnectionString}");

            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = GetTestDatabaseConnectionString(),
                    ["ConnectionStrings:SagaOrchestrationDatabase"] = GetTestDatabaseConnectionString(),
                    ["RabbitMQConfiguration:Config:HostName"] = rabbitMqHost, // [COPILOT]
                    ["RabbitMQConfiguration:Config:Port"] = rabbitMqPort.ToString(), // [COPILOT]
                    ["RabbitMQConfiguration:Config:UserName"] = RabbitMqUsername,
                    ["RabbitMQConfiguration:Config:Password"] = RabbitMqPassword,
                    ["OpenTelemetry:EnableTracing"] = "false", // ✅ disable in tests
                    ["OpenTelemetry:EnableMetrics"] = "false" // ✅ disable in tests
                })
                .AddEnvironmentVariables()
                .Build();

            configurationBuilder.AddConfiguration(Configuration);
        });

        builder.ConfigureTestServices(services =>
        {
            // ✅ Force EF to use Testcontainers DB
            services.RemoveAll<DbContextOptions<BookyWooksOrderDbContext>>();
            services.AddDbContext<BookyWooksOrderDbContext>(options =>
            {
                var testDbConnectionString = GetTestDatabaseConnectionString();
                Console.WriteLine($"[DEBUG] FORCED EF ConnectionString: {testDbConnectionString}");
                options.UseSqlServer(testDbConnectionString);
            });

            // 🔥 Disable Quartz jobs for tests
            var quartzDescriptors = services
                .Where(s => s.ServiceType.Name.Contains("Quartz"))
                .ToList();
            foreach (var descriptor in quartzDescriptors)
            {
                services.Remove(descriptor);
            }

            // Or more explicitly:
            services.AddQuartz(q => q.UseInMemoryStore());
            services.Configure<QuartzOptions>(opt =>
            {
                opt.Scheduling.IgnoreDuplicates = true;
                opt.Scheduling.OverWriteExistingData = true;
            });

            // Replace the outbox job with a no-op
            services.AddSingleton<IJob, NoOpOutboxJob>();

            // ✅ Use MassTransit Test Harness with RabbitMQ
            services.AddMassTransitTestHarness(busRegistrationConfigurator =>
            {
                ConfigureMassTransit(busRegistrationConfigurator);

                busRegistrationConfigurator.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = _rabbitMqContainer.Hostname; // [COPILOT]
                    var rabbitMqPort = _rabbitMqContainer.GetMappedPublicPort(5672); // [COPILOT]
                    Console.WriteLine($"[COPILOT][RabbitMQ] Configuring MassTransit with Host: {rabbitMqHost}, Port: {rabbitMqPort}");

                    cfg.Host(rabbitMqHost, rabbitMqPort, "/", h =>
                    {
                        h.Username(RabbitMqUsername);
                        h.Password(RabbitMqPassword);
                    });

                    // [COPILOT] Add connection diagnostics
                    //cfg.UseHealthCheck(context);

                    ConfigureEndpoints(context, cfg);
                });
            });
        });
    }
    public class NoOpOutboxJob : IJob
    {
        public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
    }
    protected abstract void ConfigureMassTransit(IBusRegistrationConfigurator busRegistrationConfigurator);

    protected virtual void ConfigureEndpoints(
        IBusRegistrationContext busRegistrationContext,
        IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator)
    {
        rabbitMqBusFactoryConfigurator.ConfigureEndpoints(busRegistrationContext);
    }

    private string GetTestDatabaseConnectionString()
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(_mssqlContainer.GetConnectionString())
        {
            InitialCatalog = "BookyWooksTest"
        };
        return builder.ConnectionString;
    }

    public new async Task DisposeAsync()
    {
        await _mssqlContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
    }
}