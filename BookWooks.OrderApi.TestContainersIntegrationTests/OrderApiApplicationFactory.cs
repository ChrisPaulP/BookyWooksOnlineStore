using BookWooks.OrderApi.Infrastructure.Data;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using OutBoxPattern;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Respawn;
using Docker.DotNet.Models;
using Microsoft.Extensions.Hosting;
using System.Reflection.Metadata;

namespace BookWooks.OrderApi.TestContainersIntegrationTests;

public class OrderApiApplicationFactory<TEntryPoint> : WebApplicationFactory<Program>, IAsyncLifetime where TEntryPoint : Program // : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string Database = "master";
    private const string Username = "sa";
    private const string Password = "yourStrong(!)Password";
    private string ConnectionString;
    private const ushort MsSqlPort = 1433;
    private readonly IContainer _mssqlContainer;

    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;
    private IConfiguration _configuration = null!;
    public HttpClient HttpClient { get; private set; } = default!;

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
    }

    public async Task InitializeAsync()
    {
        //await InitializeMssqlContainerAsync();
        await _mssqlContainer.StartAsync();

        var host = _mssqlContainer.Hostname;
        var port = _mssqlContainer.GetMappedPublicPort(MsSqlPort);

        ConnectionString = $"Server={host},{port};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=True";

        //_respawner = await CreateRespawnerAsync();
        HttpClient = CreateClient();
    }

    //private async Task InitializeMssqlContainerAsync()
    //{
    //    _mssqlContainer = new ContainerBuilder()
    //      .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
    //      .WithPortBinding(MsSqlPort, true)
    //      .WithEnvironment("ACCEPT_EULA", "Y")
    //      .WithEnvironment("SQLCMDUSER", Username)
    //      .WithEnvironment("SQLCMDPASSWORD", Password)
    //      .WithEnvironment("MSSQL_SA_PASSWORD", Password)
    //      .Build();

    //    await _mssqlContainer.StartAsync();
    //}

    private async Task<Respawner> CreateRespawnerAsync()
    {
        return await Respawner.CreateAsync(ConnectionString,
            new RespawnerOptions
            {
                TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" }
            });
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

    public async Task ResetDatabaseAsync()
    {
        var connectionString = GetDatabaseConnectionString();
        await _respawner.ResetAsync(connectionString);
    }

    private string GetDatabaseConnectionString()
    {
        return $"Server={_mssqlContainer.Hostname},{_mssqlContainer.GetMappedPublicPort(MsSqlPort)};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=True";
    }

    public new async Task DisposeAsync()
    {
        await _mssqlContainer.DisposeAsync();
    }
}

