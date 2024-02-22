using Azure.Messaging.ServiceBus;
using EventBus;
using EventBus.IntegrationEventInterfaceAbstraction;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceBus;

public class ServiceBusAzure : IEventBus
{
    //TODO: make more secured
    private string connectionString = "Endpoint=sb://blazerrestaurant.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=qLLl1deGOe4SfUowmHTYWNfTXYZIDEnv6czOEfR+HIM=";
    private readonly ITopicClient topicClient;
    private readonly IConfiguration configuration;

    public ServiceBusAzure(IConfiguration configuration)
    {
        this.configuration = configuration;

        // Initialize Azure Service Bus topic client
        var connectionString = configuration.GetConnectionString("AzureServiceBusConnection");
        var topicName = configuration["AzureServiceBus:TopicName"];
        topicClient = new TopicClient(connectionString, topicName);
    }

    public async Task Publish(IntegrationEventBase @event)
    {
        var message = new Message
        {
            MessageId = Guid.NewGuid().ToString(),
            Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
            Label = @event.GetType().Name,
        };

        try
        {
            await topicClient.SendAsync(message);
        }
        catch (Exception ex)
        {
            // Handle exceptions
        }
    }

    public async Task Subscribe<T, TH>()
        where T : IntegrationEventBase
        where TH : IIntegrationEventHandler<T>
    {
        // Implement your subscription logic
    }

    public void Unsubscribe<T, TH>()
        where T : IntegrationEventBase
        where TH : IIntegrationEventHandler<T>
    {
        // Implement your unsubscription logic
    }
}