

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

    public static IReadOnlyList<Type> RegisteredConsumers { get; private set; }
    public static IServiceCollection AddMessageBroker<TDbContext>
 (this IServiceCollection services, IConfiguration configuration, Assembly? assembly, bool useSqlServer)
 where TDbContext : DbContext
    {
        services.Configure<RabbitMQConfiguration>(configuration.GetSection("RabbitMQConfiguration:Config"));

        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<RabbitMQConfiguration>>().Value);

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
                configurator.ConfigureEndpoints(context);
            });
        });
        return services;
    }   
}

