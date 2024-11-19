

using BookyWooks.Messaging.Constants;
using BookyWooks.Messaging.RabbitMq;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Buffers.Text;
using System.Drawing;
using System.Net;
using System.Reflection;
using static MassTransit.Monitoring.Performance.BuiltInCounters;
using System.Text;
using static MassTransit.Util.ChartTable;

namespace BookyWooks.Messaging.MassTransit;

public static class Extentions
{
    private static List<string> ReceiveEndpointNames { get; } = new List<string>();
    public static IReadOnlyList<Type> RegisteredConsumers { get; private set; } = new List<Type>();
    public static IServiceCollection AddMessageBroker<TDbContext>
 (this IServiceCollection services, IConfiguration configuration, Assembly? assembly, bool useSqlServer)
 where TDbContext : DbContext
    {
        services.Configure<RabbitMQConfiguration>(configuration.GetSection("RabbitMQConfiguration:Config"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMQConfiguration>>().Value);

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

            config.AddConsumers(assembly); // Informs MassTransit about consumers but does not create any endpoints by itself.
            config.AddActivities(assembly);

            config.UsingRabbitMq((context, configurator) =>
            {
                var rabbitMqConfig = context.GetRequiredService<IOptions<RabbitMQConfiguration>>().Value;
                configurator.Host(rabbitMqConfig.HostName, "/", host =>
                {
                    host.Username(rabbitMqConfig.UserName);
                    host.Password(rabbitMqConfig.Password);
                });

                // Manual Configuration of Endpoints:
                foreach (var consumerType in RegisteredConsumers)
                {
                    string endpointName = QueueConstants.GetQueueNameForConsumer(consumerType);
                    ReceiveEndpointNames.Add(endpointName); // Store the endpoint name
                    configurator.ReceiveEndpoint(endpointName, e => // Create a new endpoint for each consumer
                    {
                        e.ConfigureConsumer(context, consumerType); // Configures the consumer for the endpoint
                    });
                }
                //Automatic Configuration of Endpoints:
                configurator.ConfigureEndpoints(context); // creates endpoints for the consumers using the default endpoint naming conventions.
                
                // Print out registered consumersfor debugging purposes
                foreach (var consumerType in RegisteredConsumers)
                {
                    Console.WriteLine($"Registered consumer: {consumerType.FullName}");
                }
                // Print out receiving endpoint names for debugging purposes
                foreach (var endpointName in ReceiveEndpointNames)
                {
                    Console.WriteLine($"Receiving endpoint: {endpointName}");
                }
            });
        });
        return services;
    }
}
// Both the AddConsumers and AddActivities methods are used to inform MassTransit about the consumers and activities that are available in the application.

//Automatic Configuration of Endpoints: The configurator.ConfigureEndpoints(context); call is a convenience method provided by MassTransit to automatically configure endpoints based on the consumers added to the configuration.
//Manual Configuration of Endpoints: However, if manually configuring each endpoint using a custom logic(like using QueueConstants.GetQueueNameForConsumer to derive endpoint names), then manually configuring endpoints is necessary.


//In the context of message-driven systems and MassTransit, an endpoint is a configuration point where messages are sent to or received from.Specifically, for a consumer, an endpoint is the connection point where the consumer listens for incoming messages.Each endpoint typically corresponds to a queue in a message broker like RabbitMQ.

//Key Components of an Endpoint
//Name: Each endpoint has a unique name that often corresponds to the name of the queue it uses in the message broker.
//Configuration: Defines how the endpoint interacts with the message broker, including details such as which queue it listens to, retry policies, and other settings.
//Consumers: The consumers that process messages received at the endpoint.