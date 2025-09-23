var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(SeriLogger.Configure);

var sagaDbConnectionString = builder.Configuration.GetConnectionString("SagaOrchestrationDatabase");
builder.Services.AddDbContext<StateMachineDbContext>(options =>
    options.UseSqlServer(sagaDbConnectionString));


builder.Services.AddMassTransit(cfg =>
{
    // Configure the outbox to ensure atomic message publishing and state changes
    cfg.AddEntityFrameworkOutbox<StateMachineDbContext>(o =>
    {
        o.UseSqlServer();
        o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
    });

    cfg.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(opt =>
    {
        opt.AddDbContext<DbContext, StateMachineDbContext>((provider, dbContextOptionsBuilder) =>
        {
            dbContextOptionsBuilder.UseSqlServer(sagaDbConnectionString,
                m => { m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name); });
        });

        opt.ConcurrencyMode = ConcurrencyMode.Optimistic;
    });

    cfg.UsingRabbitMq((context, configure) =>
    {
        var rabbitMQConfig = builder.Configuration.GetSection("RabbitMQConfiguration:Config");
        configure.Host(rabbitMQConfig["HostName"], host =>
        {
            host.Username(rabbitMQConfig["UserName"]);
            host.Password(rabbitMQConfig["Password"]);
        });

        configure.ReceiveEndpoint(QueueConstants.CreateOrderMessageQueueName, e =>
        {
            e.ConfigureSaga<OrderStateInstance>(context);
            e.UseEntityFrameworkOutbox<StateMachineDbContext>(context);
        });
    });
});

builder.Services.AddOpenTelemetryTracing(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var orderDbContext = scope.ServiceProvider.GetRequiredService<StateMachineDbContext>();
    orderDbContext.Database.Migrate();
}

app.Run();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class SagaOrchestrationProgram
{
    protected SagaOrchestrationProgram() { }
}