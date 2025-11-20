namespace SagaOrchestration.IntegrationTests;

public static class TestServiceConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure any additional services needed for testing
        services.AddSingleton(configuration);

        // Register DbContext for StateMachine and enable detailed EF logging
        ConfigureDbContext(services, configuration);

        // Add console logging with filters for MassTransit and EF Core to help diagnose outbox activity
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddFilter("MassTransit", LogLevel.Debug);
            builder.AddFilter("MassTransit.EntityFrameworkCoreIntegration", LogLevel.Debug);
            builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Debug);
            builder.AddFilter("SagaOrchestration", LogLevel.Debug);
        });
    }
    private static void ConfigureDbContext(IServiceCollection services, IConfiguration configuration)
    {
        // Remove ALL DbContext registrations to avoid conflicts
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(DbContextOptions<StateMachineDbContext>) ||
            d.ServiceType == typeof(StateMachineDbContext)).ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<StateMachineDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SagaOrchestrationDatabase"))
                   .EnableSensitiveDataLogging()
                   .LogTo(Console.WriteLine, LogLevel.Debug)
        );
    }
}