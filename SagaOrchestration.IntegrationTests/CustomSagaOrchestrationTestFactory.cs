using BookyWooks.Messaging.Constants;
using BookyWooks.Messaging.Contracts.Commands;
using IntegrationTestingSetup;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaOrchestration.Data;
using SagaOrchestration.IntegrationTests.Consumers;
using SagaOrchestration.StateInstances;
using SagaOrchestration.StateMachines;
using System.Net.Mime;
using System.Reflection;

namespace SagaOrchestration.IntegrationTests;

public class CustomSagaOrchestrationTestFactory<TEntryPoint> : TestFactoryBase<TEntryPoint> where TEntryPoint : class
{
    //protected override void ConfigureProjectSpecificServices(IServiceCollection services)
    //{
    //    // Add project-specific services here
    //    services.AddSingleton<IProject3Service, Project3Service>();
    //}

    protected override void ConfigureMassTransit(IBusRegistrationConfigurator busRegistration)
    {
        busRegistration.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(opt =>
        {
            opt.AddDbContext<DbContext, StateMachineDbContext>((provider, builder) =>
            {
                builder.UseSqlServer(SqlContainer.GetConnectionString(), m => { m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name); });
            });
            opt.ConcurrencyMode = ConcurrencyMode.Optimistic;
        });

        busRegistration.AddConsumer<CheckBookStockCommandConsumer>();

    }

    protected override void ConfigureEndpoints(IBusRegistrationContext busRegistrationContext, IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator)
    {
        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(QueueConstants.CreateOrderMessageQueueName, e =>
        {
            e.ConfigureSaga<OrderStateInstance>(busRegistrationContext);
            e.UseEntityFrameworkOutbox<StateMachineDbContext>(busRegistrationContext);
            
            
            e.DefaultContentType = new ContentType("application/json");
            e.UseRawJsonDeserializer();


            Console.WriteLine(QueueConstants.CreateOrderMessageQueueName);
        });

        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(QueueConstants.CheckBookStockCommandQueueName, e =>
        {
            e.ConfigureConsumer<CheckBookStockCommandConsumer>(busRegistrationContext);
            e.DefaultContentType = new ContentType("application/json");
            e.UseRawJsonDeserializer();

            Console.WriteLine(QueueConstants.CheckBookStockCommandQueueName);
        });
    }
}