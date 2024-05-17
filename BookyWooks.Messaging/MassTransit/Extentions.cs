

using BookyWooks.Messaging.Constants;
using BookyWooks.Messaging.RabbitMq;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace BookyWooks.Messaging.MassTransit;

public static class Extentions
{
    private static List<string> ReceiveEndpointNames { get; } = new List<string>();
    public static IReadOnlyList<Type> RegisteredConsumers { get; private set; }
    public static IServiceCollection AddMessageBroker<TDbContext>
 (this IServiceCollection services, IConfiguration configuration, Assembly? assembly, bool useSqlServer)
 where TDbContext : DbContext
    {
        services.Configure<RabbitMQConfiguration>(configuration.GetSection("RabbitMQConfiguration:Config"));

        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<RabbitMQConfiguration>>().Value);

        // Register QueueCreator with a factory method that resolves the connection string
        //services.AddSingleton<QueueCreator>(provider =>
        //{
        //    var rabbitMqConfig = provider.GetRequiredService<RabbitMQConfiguration>();
        //    return new QueueCreator(rabbitMqConfig.HostName);
        //});

        services.AddMassTransit(config =>
        {
            config.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                //o.QueryDelay = TimeSpan.FromSeconds(1);
                o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                if (useSqlServer) o.UseSqlServer().UseBusOutbox();
                else o.UsePostgres().UseBusOutbox(); 
            });

            config.SetKebabCaseEndpointNameFormatter();

            if (assembly != null)
            {
                RegisteredConsumers = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(IConsumer).IsAssignableFrom(t))
                    .ToList(); // Store the list of registered consumer types
            }

            config.AddConsumers(assembly);
            config.AddActivities(assembly);

            config.UsingRabbitMq((context, configurator) =>
            {
                var rabbitMqConfig = context.GetRequiredService<IOptions<RabbitMQConfiguration>>().Value;
                rabbitMqConfig.HostName = "amqp://localhost:5672";
                configurator.Host(new Uri(rabbitMqConfig.HostName), host =>
                {
                    host.Username(rabbitMqConfig.UserName);
                    host.Password(rabbitMqConfig.Password);
                });
                // Call CreateQueues here to ensure queues are created
                //var queueCreator = context.GetRequiredService<QueueCreator>();
                //queueCreator.CreateQueues();

                // Print out registered consumers
                foreach (var consumerType in RegisteredConsumers)
                {
                    Console.WriteLine($"Registered consumer: {consumerType.FullName}");
                }

                // Configure receive endpoints for registered consumers
                foreach (var consumerType in RegisteredConsumers)
                {
                    string endpointName = QueueConstants.GetQueueNameForConsumer(consumerType);
                    ReceiveEndpointNames.Add(endpointName); // Store the endpoint name
                    configurator.ReceiveEndpoint(endpointName, e =>
                    {
                        e.ConfigureConsumer(context, consumerType);
                    });
                }

                // Print out receiving endpoint names
                foreach (var endpointName in ReceiveEndpointNames)
                {
                    Console.WriteLine($"Receiving endpoint: {endpointName}");
                }

                configurator.ConfigureEndpoints(context);
            });
        });
        return services;
    }
   
    //private static void ConfigureReceiveEndpoint<TConsumer>(
    //    IRabbitMqBusFactoryConfigurator configurator,
    //    IReceiveConfigurator<TConsumer> context,
    //    Type consumerType)
    //    where TConsumer : class, IConsumer
    //{
    //    configurator.ReceiveEndpoint(QueueConstants.GetQueueNameForConsumer(consumerType), e =>
    //    {
    //        e.ConfigureConsumer<TConsumer>(context);
    //    });
    //}
}

