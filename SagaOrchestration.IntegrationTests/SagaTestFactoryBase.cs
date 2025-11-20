using BookyWooks.Messaging.Constants;
using BookyWooks.Messaging.Contracts.Commands;
using BookyWooks.Messaging.Messages.InitialMessage;
using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using FluentAssertions.Common;
using global::BookWooks.OrderApi.Infrastructure.AiServices;
using global::BookWooks.OrderApi.TestContainersIntegrationTests.TestSetup;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SagaOrchestration.Data;
using SagaOrchestration.StateInstances;
using SagaOrchestration.StateMachines;
using System.Net;
using System.Net.Http.Headers;
using Testcontainers.MsSql;
using Testcontainers.Qdrant;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace SagaOrchestration.IntegrationTests;

public abstract class SagaTestFactoryBase<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
    where TEntryPoint : class
{
    private readonly INetwork _network;
    private readonly TestContainerBuilder _containerBuilder;
    private static readonly object _lock = new();
    private static bool _containersStarted;
    private string _testDatabaseConnectionString = default!;

    public readonly MsSqlContainer SqlContainer;
    private readonly RabbitMqContainer RabbitMqContainer;

    protected IConfiguration Configuration { get; private set; } = default!;

    public string RabbitMqHost => RabbitMqContainer.Hostname;
    public ushort RabbitMqPort => RabbitMqContainer.GetMappedPublicPort(5672);

    protected SagaTestFactoryBase()
    {
        try
        {
            var timestamp = DateTime.UtcNow.Ticks;
            _network = new NetworkBuilder()
                .WithName($"saga-integration-tests")
                .Build();

            _containerBuilder = new TestContainerBuilder(_network);

            // Initialize containers
            SqlContainer = _containerBuilder.BuildSqlContainer();
            RabbitMqContainer = _containerBuilder.BuildRabbitMqContainer();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to initialize containers: {ex.Message}");
            throw;
        }
    }

    public async Task InitializeAsync()
    {
        //if (ShouldInitializeContainers())
        //{
            await InitializeContainersAsync();
            await InitializeDatabaseAsync();
        //}

        // Apply EF migrations to ensure outbox/inbox/saga tables exist for tests
        try
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StateMachineDbContext>();
            Console.WriteLine("[DEBUG] Applying migrations for StateMachineDbContext...");
            await db.Database.MigrateAsync();
            Console.WriteLine("[DEBUG] Migrations applied.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to apply migrations: {ex.Message}");
            throw;
        }
        EndpointConvention.Map<OrderCreatedMessage>(new Uri($"queue:{QueueConstants.CreateOrderMessageQueueName}"));
        EndpointConvention.Map<CompletePaymentCommand>(new Uri($"queue:{QueueConstants.CompletePaymentCommandQueueName}"));

    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(ConfigureAppSettings);
        builder.ConfigureTestServices(ConfigureTestServices);
    }

    private void ConfigureAppSettings(IConfigurationBuilder configBuilder)
    {       
        var connectionStrings = new Dictionary<string, string>
        {
            ["ConnectionStrings:SagaOrchestrationDatabase"] = _testDatabaseConnectionString ?? SqlContainer.GetConnectionString(),          
            ["RabbitMQConfiguration:Config:HostName"] = RabbitMqHost,
            ["RabbitMQConfiguration:Config:Port"] = RabbitMqPort.ToString(),
            ["RabbitMQConfiguration:Config:UserName"] = ContainerConfiguration.RabbitMqUsername,
            ["RabbitMQConfiguration:Config:Password"] = ContainerConfiguration.RabbitMqPassword
        };

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(connectionStrings)
            .Build();
    }

    private void ConfigureTestServices(IServiceCollection services)
    {
        TestServiceConfiguration.ConfigureServices(services, Configuration);
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

            await Task.WhenAll(
                RabbitMqContainer.StartAsync()
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
        try
        {
            await EnsureDatabaseCreatedAsync(SqlContainer.GetConnectionString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to initialize database: {ex.Message}");
            throw;
        }
    }

    private async Task EnsureDatabaseCreatedAsync(string masterConnection)
    {
        var maxRetries = 30;
        var retryDelay = TimeSpan.FromSeconds(1);
        var builder = new SqlConnectionStringBuilder(masterConnection)
        {
            InitialCatalog = "StateMachineDb"
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



    protected virtual void ConfigureEndpoints(IBusRegistrationContext ctx, IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.ConfigureEndpoints(ctx);
    }

    protected abstract void ConfigureMassTransit(IBusRegistrationConfigurator cfg);

    public async Task DisposeAsync()
    {

        await Task.WhenAll(
            SqlContainer.DisposeAsync().AsTask(),
            RabbitMqContainer.DisposeAsync().AsTask()
        );
        await _network.DisposeAsync();

    }

    /// <summary>
    /// Cleans up all test data from the database to ensure test isolation
    /// </summary>
    //public async Task CleanupTestDataAsync()
    //{
    //    using var scope = Services.CreateScope();
    //    var context = scope.ServiceProvider.GetRequiredService<StateMachineDbContext>();

    //    try
    //    {
    //        // Clean up in the correct order to avoid foreign key constraints
    //        await context.Database.ExecuteSqlRawAsync("DELETE FROM OutboxMessage");
    //        await context.Database.ExecuteSqlRawAsync("DELETE FROM InboxState");
    //        await context.Database.ExecuteSqlRawAsync("DELETE FROM OutboxState");
    //        await context.Database.ExecuteSqlRawAsync("DELETE FROM OrderStateInstance");

    //        await context.SaveChangesAsync();
    //        Console.WriteLine("[DEBUG] Test data cleanup completed");
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"[ERROR] Failed to cleanup test data: {ex.Message}");
    //        throw;
    //    }
    //}

    ///// <summary>
    ///// Resets the MassTransit test harness state
    ///// </summary>
    //public async Task ResetTestHarnessAsync()
    //{
    //    try
    //    {
    //        var harness = Services.GetRequiredService<ITestHarness>();

    //        // Stop the harness if it's running (we'll check this by trying to stop it)
    //        try
    //        {
    //            await harness.Stop();
    //        }
    //        catch (InvalidOperationException)
    //        {
    //            // Harness wasn't started, ignore
    //        }

    //        // Wait a moment to ensure complete shutdown
    //        await Task.Delay(100);

    //        Console.WriteLine("[DEBUG] Test harness reset completed");
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"[WARNING] Failed to reset test harness: {ex.Message}");
    //        // Don't throw - this is cleanup code
    //    }
    //}
}

