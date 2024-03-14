using Autofac;
using Autofac.Core;
using EventBus;
using EventBus.EventBusSubscriptionsManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Serilog;
namespace RabbitMQ;

public class AutofacRabbitMQModule : Module
{
    private readonly IConfiguration _configuration;
    public AutofacRabbitMQModule(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    protected override void Load(ContainerBuilder builder)
    {
        RegisterRabbitMQConnection(builder);
        RegisterRabbitMQConfiguration(builder);
        RegisterEventBusRabbitMQ(builder);
    }
    private static ConnectionFactory GetConnectionValues(RabbitMQConfiguration rabbitMQConfiguration)
    {
        var connectionFactory = new ConnectionFactory()
        {
            HostName = rabbitMQConfiguration.HostName,
            DispatchConsumersAsync = true,//,
            Port = rabbitMQConfiguration.Port,
            //VirtualHost = rabbitMQConfiguration.VirtualHost
        };
        if (!string.IsNullOrEmpty(rabbitMQConfiguration.UserName))
        {
            connectionFactory.UserName = rabbitMQConfiguration.UserName;
        }
        if (!string.IsNullOrEmpty(rabbitMQConfiguration.Password))
        {
            connectionFactory.Password = rabbitMQConfiguration.Password;
        }
        return connectionFactory;
    }
    public void RegisterRabbitMQConnection(ContainerBuilder builder)
    {
        builder.Register(sp =>
        {
            var logger = sp.Resolve<ILogger>();
            var rabbitMQConfiguration = sp.Resolve<RabbitMQConfiguration>();
            var rabbitMQAdditionalConfiguration = sp.Resolve<IOptions<RabbitMQAdditionalConfiguration>>()!;
            var factory = GetConnectionValues(rabbitMQConfiguration);

            return new RabbitMQConnection(factory, logger, rabbitMQAdditionalConfiguration);
        }).As<IRabbitMQConnection>().SingleInstance();
    }
    public void RegisterRabbitMQConfiguration(ContainerBuilder builder)
    {
        //    To make the computer feel better, you can say, "Don't worry, computer, I'm sure these pages will always have something written on them." You do this by using a special sign!.It's like telling the computer, "Trust me, there will always be something here!"
        //    So, the warning is just a reminder to be careful, and using ! is like promising the computer that everything will be okay.Just be sure you're keeping your promise! 
        builder.RegisterInstance(_configuration.GetSection("RabbitMQConfiguration:Config").Get<RabbitMQConfiguration>()!)
               .SingleInstance();

        builder.RegisterInstance(_configuration.GetSection("RabbitMQConfiguration:AdditionalConfig").Get<RabbitMQAdditionalConfiguration>()!)
               .SingleInstance();
    }
    public void RegisterEventBusRabbitMQ(ContainerBuilder builder)
    {
        builder.Register(sp =>
        {
            var logger = sp.Resolve<ILogger>();
            var eventBusSubscriptionsManager = sp.Resolve<IEventBusSubscriptionsManager>();
            var rabbitMQConnection = sp.Resolve<IRabbitMQConnection>();
            var serviceProvider = sp.Resolve<IServiceScopeFactory>();
            var rabbitMQAdditionalConfiguration = sp.Resolve<RabbitMQAdditionalConfiguration>();

            return new EventBusRabbitMQ(rabbitMQConnection, eventBusSubscriptionsManager, logger, serviceProvider, rabbitMQAdditionalConfiguration);
        }).As<IEventBus>().SingleInstance();
    }
   
}



   

