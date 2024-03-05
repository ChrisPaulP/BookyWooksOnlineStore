using BookWooks.OrderApi.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OutBoxPattern;
using Respawn;

using System.Data.Common;


namespace BookyWooks.OrderApi.IntegrationTests;

public class OrderApiApplicationTestFactory<TEntryPoint> : WebApplicationFactory<Program>, IAsyncLifetime where TEntryPoint : Program 
{

    private string _connectionString;
    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;
    private IConfiguration _configuration = null!;
    public HttpClient HttpClient { get; private set; } = default!;
    public async Task InitializeAsync()
    {
        GetDatabaseConnectionString();     
        _dbConnection = new SqlConnection(_connectionString);
        await _dbConnection.OpenAsync();
        _respawner = Respawner.CreateAsync(_connectionString,
                new RespawnerOptions
                {
                    TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" }
                }).GetAwaiter().GetResult();

        HttpClient = CreateClient();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var configuration = new ConfigurationBuilder()
               .AddJsonFile("integrationtestsappsettings.json")
               .AddEnvironmentVariables()
               .Build();
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            configurationBuilder.AddConfiguration(configuration);
        });

        builder.ConfigureServices(ConfigureDatabaseServices);
    }

    private void ConfigureDatabaseServices(IServiceCollection services)
    {
        RemoveDbContextOptions<BookyWooksOrderDbContext>(services);
        RemoveDbContextOptions<IntegrationEventLogDbContext>(services);

        AddDbContext<BookyWooksOrderDbContext>(services);
        AddDbContext<IntegrationEventLogDbContext>(services);
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
    private void GetDatabaseConnectionString()
    {
        _configuration = Services.GetRequiredService<IConfiguration>();
        _connectionString = _configuration.GetConnectionString("DefaultConnection");
    }
    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }
    public new async Task DisposeAsync()
    {
        await _dbConnection.CloseAsync();
    }
}


