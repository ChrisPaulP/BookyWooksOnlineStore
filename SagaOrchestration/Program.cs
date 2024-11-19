using System.Reflection;
using Serilog;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestration.Data;
using SagaOrchestration.StateInstances;
using SagaOrchestration.StateMachines;
using BookyWooks.Messaging.Constants;
using Microsoft.Extensions.Hosting;
using MassTransit.Configuration;
using Logging;
using Microsoft.AspNetCore.Builder;
using SagaOrchestration;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog(SeriLogger.Configure);

// Configure services
builder.Services.AddMassTransit(cfg =>
{
    // Configure the outbox to ensure atomic message publishing and state changes
    cfg.AddEntityFrameworkOutbox<StateMachineDbContext>(o =>
    {
        o.UseSqlServer();
        o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
    });

    // Add the saga state machine with Entity Framework repository
    cfg.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(opt =>
    {
        // Configure the saga repository to use the same DbContext
        opt.AddDbContext<DbContext, StateMachineDbContext>((provider, dbContextOptionsBuilder) =>
        {
            dbContextOptionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("SagaOrchestrationDatabase"),
                m => { m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name); });
        });

        opt.ConcurrencyMode = ConcurrencyMode.Optimistic;
    });

    cfg.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(configure =>
    {
        var rabbitMQConfig = builder.Configuration.GetSection("RabbitMQConfiguration:Config");
        configure.Host(rabbitMQConfig["HostName"], host =>
        {
            host.Username(rabbitMQConfig["UserName"]);
            host.Password(rabbitMQConfig["Password"]);
        });

        // Configure the receive endpoint for the saga
        configure.ReceiveEndpoint(QueueConstants.CreateOrderMessageQueueName, e =>
        {
            e.ConfigureSaga<OrderStateInstance>(provider);
            e.UseEntityFrameworkOutbox<StateMachineDbContext>(provider); // Apply the Entity Framework outbox middleware
        });
    }));
});

// Register the StateMachineDbContext with the dependency injection container
builder.Services.AddDbContext<StateMachineDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenTelemetryTracing(builder.Configuration);

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var orderDbContext = serviceProvider.GetRequiredService<StateMachineDbContext>();
    orderDbContext.Database.Migrate();
}

app.Run();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program
{
    protected Program() { }
}