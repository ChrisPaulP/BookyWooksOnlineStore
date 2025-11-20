using BookWooks.OrderApi.TestContainersIntegrationTests.TestSetup;
using BookyWooks.Messaging.Constants;
using BookyWooks.Messaging.Contracts.Commands;

using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaOrchestration.Data;
using SagaOrchestration.IntegrationTests.Consumers;
using SagaOrchestration.StateInstances;
using SagaOrchestration.StateMachines;
using System.Net.Mime;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SagaOrchestration.IntegrationTests;

public class CustomSagaOrchestrationTestFactory<TEntryPoint> : SagaTestFactoryBase<TEntryPoint> where TEntryPoint : class
{
    //protected override void ConfigureProjectSpecificServices(IServiceCollection services)
    //{
    //    // Add project-specific services here
    //    services.AddSingleton<IProject3Service, Project3Service>();
    //}

    protected override void ConfigureMassTransit(IBusRegistrationConfigurator busRegistration)
    {
        // Configure the outbox
        busRegistration.AddEntityFrameworkOutbox<StateMachineDbContext>(o =>
        {
            o.UseSqlServer();
            o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
            // Reduced QueryDelay for faster tests
            o.QueryDelay = TimeSpan.FromMilliseconds(100);
        });

        busRegistration.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(opt =>
        {
            opt.AddDbContext<DbContext, StateMachineDbContext>((provider, builder) =>
            {
                builder.UseSqlServer(SqlContainer.GetConnectionString(), m => { m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name); });
            });
            opt.ConcurrencyMode = ConcurrencyMode.Optimistic;
        });

        busRegistration.AddConsumer<CheckBookStockCommandConsumer>();
        busRegistration.AddConsumer<CompletePaymentCommandTestConsumer>();
    }

    protected override void ConfigureEndpoints(IBusRegistrationContext busRegistrationContext, IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator)
    {
        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(QueueConstants.CreateOrderMessageQueueName, e =>
        {
            // Ensure the EF outbox middleware is applied before the saga so the outgoing messages are captured as part of the same DB transaction
            e.UseEntityFrameworkOutbox<StateMachineDbContext>(busRegistrationContext);
            e.ConfigureSaga<OrderStateInstance>(busRegistrationContext);
            e.DefaultContentType = new ContentType("application/json");
            e.UseRawJsonDeserializer();


            Console.WriteLine(QueueConstants.CreateOrderMessageQueueName);
        });

        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(QueueConstants.CheckBookStockCommandQueueName, e =>
        {
            //e.UseEntityFrameworkOutbox<StateMachineDbContext>(busRegistrationContext);
            e.ConfigureConsumer<CheckBookStockCommandConsumer>(busRegistrationContext);
            e.DefaultContentType = new ContentType("application/json");
            e.UseRawJsonDeserializer();

            Console.WriteLine(QueueConstants.CheckBookStockCommandQueueName);
        });

        // Add endpoint for CompletePaymentCommand
        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(QueueConstants.CompletePaymentCommandQueueName, e =>
        {
            // For testing, you can either:
            // 1. Configure a test consumer that handles the command
            // 2. Or just configure the endpoint to consume and discard messages
            e.ConfigureConsumer<CompletePaymentCommandTestConsumer>(busRegistrationContext);
            e.DefaultContentType = new ContentType("application/json");
            e.UseRawJsonDeserializer();

            // Option 1: If you have a test consumer
            // e.ConfigureConsumer<CompletePaymentCommandTestConsumer>(busRegistrationContext);

            // Option 2: Simple message handler for testing
            //e.Handler<CompletePaymentCommand>(async context =>
            //{
            //    Console.WriteLine($"CompletePaymentCommand received for CorrelationId: {context.Message.CorrelationId}");
            //    // In a real test, you might want to publish a PaymentCompletedEvent or PaymentFailedEvent
            //    // depending on what you're testing
            //});

            Console.WriteLine(QueueConstants.CompletePaymentCommandQueueName);
        });
    }
}