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
using SagaOrchestration;
using MassTransit.Configuration;



IHost host = Host.CreateDefaultBuilder(args)

    .ConfigureServices((hostContext, services) =>
    {
        var rabbitMQConfig = hostContext.Configuration.GetSection("RabbitMQConfiguration:Config");
        var config = hostContext.Configuration.GetSection("RabbitMQUrl");
        var host = rabbitMQConfig["Host"];
        services.AddMassTransit(cfg =>
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
                opt.AddDbContext<DbContext, StateMachineDbContext>((provider, builder) =>
                {
                    builder.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection"),
                        m => { m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name); });
                });

                opt.ConcurrencyMode = ConcurrencyMode.Optimistic;
            });

            cfg.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(configure =>
            {
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
            //cfg.UsingRabbitMq((context, configurator) =>
            //{
            //    configurator.Host(new Uri(rabbitMQConfig["HostName"]), host =>
            //    {
            //        host.Username(rabbitMQConfig["UserName"]);
            //        host.Password(rabbitMQConfig["Password"]);
            //    });
            //    //configurator.UseInMemoryOutbox(context);
            //    configurator.ReceiveEndpoint(QueueConstants.CreateOrderMessageQueueName, e =>
            //    {
            //        e.ConfigureSaga<OrderStateInstance>(context);
            //        //e.UseInMemoryOutbox(context);
            //    });
            //});
        });
        // Register the StateMachineDbContext with the dependency injection container
        services.AddDbContext<StateMachineDbContext>(options =>
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection")));

        //services.AddOpenTelemetryTracing(hostContext.Configuration);

        services.AddHostedService<Worker>();
    })
    .UseSerilog((_, config) => config.ReadFrom.Configuration(_.Configuration))
    .Build();

using (var scope = host.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var orderDbContext = serviceProvider.GetRequiredService<StateMachineDbContext>();
    orderDbContext.Database.Migrate();
}

host.Run();