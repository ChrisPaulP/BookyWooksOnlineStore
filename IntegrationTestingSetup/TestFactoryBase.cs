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
        await DatabaseExtentions.ClearData(context);
        await DatabaseExtentions.SeedAsync(context);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var rabbitMqHost = _rabbitMqContainer.Hostname;
            var rabbitMqPort = _rabbitMqContainer.GetMappedPublicPort(5672);
            var rabbitMqConnectionString = $"amqp://{RabbitMqUsername}:{RabbitMqPassword}@{rabbitMqHost}:{rabbitMqPort}/";

            Console.WriteLine($"[COPILOT][RabbitMQ] Host: {rabbitMqHost}");
            Console.WriteLine($"[COPILOT][RabbitMQ] Port: {rabbitMqPort}");
            Console.WriteLine($"[COPILOT][RabbitMQ] ConnectionString: {rabbitMqConnectionString}");

            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = GetTestDatabaseConnectionString(),
                    ["ConnectionStrings:SagaOrchestrationDatabase"] = GetTestDatabaseConnectionString(),
                    ["RabbitMQConfiguration:Config:HostName"] = rabbitMqHost,
                    ["RabbitMQConfiguration:Config:Port"] = rabbitMqPort.ToString(),
                    ["RabbitMQConfiguration:Config:UserName"] = RabbitMqUsername,
                    ["RabbitMQConfiguration:Config:Password"] = RabbitMqPassword,
                    ["OpenTelemetry:EnableTracing"] = "false",
                    ["OpenTelemetry:EnableMetrics"] = "false"
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
                    var rabbitMqHost = _rabbitMqContainer.Hostname;
                    var rabbitMqPort = _rabbitMqContainer.GetMappedPublicPort(5672);
                    Console.WriteLine($"[COPILOT][RabbitMQ] MassTransit Host: {rabbitMqHost}, Port: {rabbitMqPort}");

                    cfg.Host(rabbitMqHost, rabbitMqPort, "/", h =>
                    {
                        h.Username(RabbitMqUsername);
                        h.Password(RabbitMqPassword);
                    });

                    // [COPILOT] Log endpoint configuration
                    cfg.ConnectBusObserver(new LoggingBusObserver());

                    ConfigureEndpoints(context, cfg);
                });
            });
        });
    }

    // Update the LoggingBusObserver class to match the interface signature
    private class LoggingBusObserver : IBusObserver
    {
        public void PostCreate(IBus bus)
        {
            Console.WriteLine("[COPILOT][RabbitMQ] Bus created");
        }

        public void CreateFaulted(Exception exception)
        {
            Console.WriteLine($"[COPILOT][RabbitMQ] Bus creation faulted: {exception.Message}");
        }

        public Task PreStart(IBus bus)
        {
            Console.WriteLine("[COPILOT][RabbitMQ] Bus starting");
            return Task.CompletedTask;
        }

        public Task PostStart(IBus bus, Task<BusReady> busReady)
        {
            Console.WriteLine("[COPILOT][RabbitMQ] Bus started");
            return Task.CompletedTask;
        }

        public Task StartFaulted(IBus bus, Exception exception)
        {
            Console.WriteLine($"[COPILOT][RabbitMQ] Bus start faulted: {exception.Message}");
            return Task.CompletedTask;
        }

        public Task PreStop(IBus bus)
        {
            Console.WriteLine("[COPILOT][RabbitMQ] Bus stopping");
            return Task.CompletedTask;
        }

        public Task PostStop(IBus bus)
        {
            Console.WriteLine("[COPILOT][RabbitMQ] Bus stopped");
            return Task.CompletedTask;
        }

        public Task StopFaulted(IBus bus, Exception exception)
        {
            Console.WriteLine($"[COPILOT][RabbitMQ] Bus stop faulted: {exception.Message}");
            return Task.CompletedTask;
        }
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