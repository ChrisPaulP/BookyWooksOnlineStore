namespace SagaOrchestration.IntegrationTests;

public class CustomSagaOrchestrationTestFactory<TEntryPoint> : SagaWebApplicationFactory<TEntryPoint> where TEntryPoint : class
{
    protected override void ConfigureMassTransit(IBusRegistrationConfigurator busRegistration)
    {
        // Configure the outbox
        busRegistration.AddEntityFrameworkOutbox<StateMachineDbContext>(o =>
        {
            o.UseSqlServer();
            o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
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
            e.ConfigureConsumer<CheckBookStockCommandConsumer>(busRegistrationContext);
            e.DefaultContentType = new ContentType("application/json");
            e.UseRawJsonDeserializer();

            Console.WriteLine(QueueConstants.CheckBookStockCommandQueueName);
        });

        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(QueueConstants.CompletePaymentCommandQueueName, e =>
        {
            e.ConfigureConsumer<CompletePaymentCommandTestConsumer>(busRegistrationContext);
            e.DefaultContentType = new ContentType("application/json");
            e.UseRawJsonDeserializer();

            //Simple message handler for testing
            //e.Handler<CompletePaymentCommand>(async context =>
            //{
            //    Console.WriteLine($"CompletePaymentCommand received for CorrelationId: {context.Message.CorrelationId}");
            //    // In a real test, you might want to publish a PaymentCompletedEvent or PaymentFailedEvent
            //    // depending on what you're testing
            //});
        });
    }
}