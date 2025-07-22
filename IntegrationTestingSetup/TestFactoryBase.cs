using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using IntegrationTestingSetup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;

public abstract class TestFactoryBase<TEntryPoint>
    : WebApplicationFactory<TEntryPoint>, IAsyncLifetime where TEntryPoint : class
{
    protected readonly MsSqlContainer _mssqlContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;
    protected IConfiguration Configuration { get; private set; }

    private const string RabbitMqUsername = "guest";
    private const string RabbitMqPassword = "guest";

    private const string TestDatabaseName = "BookyWooksTest";

    protected TestFactoryBase()
    {
        _mssqlContainer = IntegrationTestingSetupExtensions.CreateMsSqlContainer();
        _rabbitMqContainer = IntegrationTestingSetupExtensions.CreateRabbitMqContainer();
    }

    public async Task InitializeAsync()
    {
        await _mssqlContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        await EnsureTestDatabaseCreatedAsync();
    }

    private async Task EnsureTestDatabaseCreatedAsync()
    {
        var connectionString = _mssqlContainer.GetConnectionString();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = $@"
            IF DB_ID('{TestDatabaseName}') IS NULL
            BEGIN
                CREATE DATABASE [{TestDatabaseName}];
            END";
        await cmd.ExecuteNonQueryAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var testConfig = new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = $"{_mssqlContainer.GetConnectionString()};Database={TestDatabaseName}",
                ["ConnectionStrings:SagaOrchestrationDatabase"] = $"{_mssqlContainer.GetConnectionString()};Database={TestDatabaseName}",
                ["RabbitMQConfiguration:Config:HostName"] = _rabbitMqContainer.Hostname,
                ["RabbitMQConfiguration:Config:UserName"] = RabbitMqUsername,
                ["RabbitMQConfiguration:Config:Password"] = RabbitMqPassword,

                // ✅ Disable OpenTelemetry for integration tests
                ["OpenTelemetry:TracingEnabled"] = "false",
                ["OpenTelemetry:MetricsEnabled"] = "false"
            };

            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfig)
                .AddEnvironmentVariables() // allow CI/CD overrides if needed
                .Build();

            configurationBuilder.AddConfiguration(Configuration);
        });

        builder.ConfigureTestServices(services =>
        {
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

            // If you need Redis override in the future, call: OverrideRedis(services, Configuration);
        });
    }

    private static void OverrideRedis(IServiceCollection services, IConfiguration configuration)
    {
        var descriptors = services
            .Where(s => s.ServiceType == typeof(IDistributedCache) || s.ServiceType == typeof(RedisCacheOptions))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }

        var redisConnectionString = configuration.GetValue<string>("ConnectionStrings:Redis")
            ?? throw new ArgumentNullException("Redis connection string not configured");

        Console.WriteLine($"[DEBUG] Overriding Redis with: {redisConnectionString}");

        var redisConfiguration = new RedisCacheOptions
        {
            ConfigurationOptions = new ConfigurationOptions
            {
                AbortOnConnectFail = true,
                EndPoints = { redisConnectionString }
            }
        };

        services.AddSingleton(redisConfiguration);
        services.AddSingleton<IDistributedCache>(sp =>
        {
            var options = sp.GetRequiredService<RedisCacheOptions>();
            return new RedisCache(options);
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
