using EventBus;
using EventBus.EventBusSubscriptionsManager;
using EventBus.IntegrationEventInterfaceAbstraction;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Serilog;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ;

public class EventBusRabbitMQ : IEventBus, IDisposable
{
    private readonly IRabbitMQConnection _persistentConnection;
    private readonly ILogger _logger;
    private readonly IEventBusSubscriptionsManager _subsManager;
    private readonly int _retryCount;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private IModel _consumerChannel;
    private string _queueName;
    private string _brokerName;

    public EventBusRabbitMQ(IRabbitMQConnection persistentConnection, IEventBusSubscriptionsManager subsManager,
            ILogger logger, IServiceScopeFactory serviceScopeFactory, RabbitMQAdditionalConfiguration rabbitMQAdditionalConfig)
    {
        _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _subsManager = subsManager ?? throw new ArgumentNullException(nameof(subsManager));

        _queueName = rabbitMQAdditionalConfig.QueueName;
        _brokerName = rabbitMQAdditionalConfig.BrokerName;
        _consumerChannel = CreateConsumerChannel();

        _retryCount = rabbitMQAdditionalConfig.Retry;
        _logger = logger;
        _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
    }

    private void SubsManager_OnEventRemoved(object sender, string eventName)
    {
        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        using (var channel = _persistentConnection.CreateModel())
        {
            channel.QueueUnbind(queue: _queueName,
                exchange: _brokerName,
                routingKey: eventName);

            if (_subsManager.IsEmpty)
            {
                _queueName = string.Empty;
                _consumerChannel.Close();
            }
        }
    }
    public void Dispose()
    {
        if (_consumerChannel != null)
        {
            _consumerChannel.Dispose();
        }

        _subsManager.Clear();
    }

    public async Task Publish(IntegrationEventBase _event)
    {
        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        var policy = RetryPolicy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
            {
                _logger.Warning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", _event.Id, $"{time.TotalSeconds:n1}", ex.Message);
            });

        var eventName = _event.GetType().Name;

        using (var channel = _persistentConnection.CreateModel())
        {

            _logger.Information("Declaring RabbitMQ exchange to publish event: {EventId}", _event.Id);

            channel.ExchangeDeclare(exchange: _brokerName, type: "direct");

            var message = JsonConvert.SerializeObject(_event);
            var body = Encoding.UTF8.GetBytes(message);

            policy.Execute(() =>
            {
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2;

                _logger.Information("Publishing event to RabbitMQ: {EventId}", _event.Id);

                channel.BasicPublish(
                    exchange: _brokerName,
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
            });
        }
    }

    public async Task Subscribe<T, TH>()
        where T : IntegrationEventBase
        where TH : IIntegrationEventHandler<T>
    {
        var eventName = _subsManager.GetEventKey<T>();
        DoInternalSubscription(eventName);

        _logger.Information("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);

        _subsManager.AddSubscription<T, TH>();
        StartBasicConsume();
    }
    private void DoInternalSubscription(string eventName)
    {
        var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
        if (!containsKey)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueBind(queue: _queueName,
                                  exchange: _brokerName,
                                  routingKey: eventName);
            }
        }
    }
    private void StartBasicConsume()
    {
        _logger.Information("Starting RabbitMQ basic consume");

        if (_consumerChannel != null)
        {
            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

            consumer.Received += Consumer_Received;

            _consumerChannel.BasicConsume(
                queue: _queueName,
                autoAck: false,
                consumer: consumer);
        }
        else
        {
            _logger.Error("StartBasicConsume can't call on _consumerChannel == null");
        }
    }

    private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

        try
        {
            if (message.ToLowerInvariant().Contains("throw-fake-exception"))
            {
                throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
            }

            await ProcessEvent(eventName, message);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "----- ERROR Processing message \"{Message}\"", message);
        }

        _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
    }
    private IModel CreateConsumerChannel()
    {
        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        //_logger.Information("Creating RabbitMQ consumer channel");

        var channel = _persistentConnection.CreateModel();

        channel.ExchangeDeclare(exchange: _brokerName,
                                type: "direct");

        channel.QueueDeclare(queue: _queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        channel.CallbackException += (sender, ea) =>
        {
            _logger.Warning(ea.Exception, "Recreating RabbitMQ consumer channel");

            _consumerChannel.Dispose();
            _consumerChannel = CreateConsumerChannel();
            StartBasicConsume();
        };

        return channel;
    }
    private async Task ProcessEvent(string eventName, string message)
    {
        _logger.Information("Processing RabbitMQ event: {EventName}", eventName);

        if (_subsManager.HasSubscriptionsForEvent(eventName))
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var subscriptions = _subsManager.GetHandlersForEvent(eventName);

                foreach (var subscription in subscriptions)
                {
                    var handler = scope.ServiceProvider.GetRequiredService(subscription.HandlerType);

                    if (handler == null)
                        continue;

                    var eventType = _subsManager.GetEventTypeByName(eventName);
                    var Event = JsonConvert.DeserializeObject(message, eventType);
                    var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

                    await Task.Yield();
                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { Event });

                }
            }
        }
        else
        {
            _logger.Warning("No subscription for RabbitMQ event: {EventName}", eventName);
        }
    }
}
//Notes:
//
//The Dependency Injection(DI) container provides us with the concrete instance of the handler, which is the object capable of processing the event. This handler instance is retrieved based on the type specified by subscription.HandlerType.

//On the other hand, MakeGenericType dynamically creates a specific type representing the handler for the event being processed. This is necessary because the Handle method is defined in a generic interface (IIntegrationEventHandler<>), and the specific type needs to be known at runtime to correctly invoke this method.

//So, in summary:

//The DI container gives us the actual instance of the handler.
//MakeGenericType helps us dynamically create the correct type to ensure that we can invoke the Handle method on the handler instance obtained from the DI container, based on the type of event being processed.
//Together, these mechanisms allow for flexible and dynamic handling of events in the application, enabling decoupling and extensibility.