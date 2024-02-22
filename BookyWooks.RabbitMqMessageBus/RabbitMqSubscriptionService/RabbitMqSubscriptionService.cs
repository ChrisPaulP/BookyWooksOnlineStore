using BookyWooks.MessageBus;
using BookyWooks.MessageBus.EventBusSubscriptionsManager;
using BookyWooks.MessageBus.IntegrationEventSetUp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BookyWooks.RabbitMqMessageBus.RabbitMqSubscriptionService
{
    public class RabbitMqSubscriptionService : IEventBusSubscription, IDisposable
    {
        const string BROKER_NAME = "lostChapters_event_bus";

        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IRabbitMqConnectionSetUp _rabbitMQConnectionManagementService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMqSubscriptionService> _logger;
        private readonly int _retryCount;
        private readonly IRabbitMqBaseConsumer _rabbitMqBaseConsumer;

        private IModel _consumerChannel;
        private string _queueName;
        private string _queueNameForAzureFunction;

        public RabbitMqSubscriptionService(
             IEventBusSubscriptionsManager subsManager,
             IRabbitMqConnectionSetUp rabbitMQConnectionManagementService,
             RabbitMqConfiguration rabbitMqConfiguration,
             IServiceProvider serviceProvider,
             IRabbitMqBaseConsumer rabbitMqBaseConsumer,
             ILogger<RabbitMqSubscriptionService> logger)
        {
            _subsManager = subsManager;
            _rabbitMQConnectionManagementService = rabbitMQConnectionManagementService;
            _serviceProvider = serviceProvider;
            _rabbitMqConfiguration = rabbitMqConfiguration;
            _logger = logger;
            _queueName = rabbitMqConfiguration.SubscriptionClientName;
            _consumerChannel = CreateConsumerChannel();
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
            _rabbitMqBaseConsumer = rabbitMqBaseConsumer;
        }
        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }
            _subsManager.Clear();
        }
        private IModel? CreateConsumerChannel()
        {
            if (!_rabbitMQConnectionManagementService.IsConnected)
            {
                _rabbitMQConnectionManagementService.TryConnect();
            }

            _logger.LogTrace("Creating RabbitMQ consumer channel");

            var channel = _rabbitMQConnectionManagementService.CreateModel();

            channel.ExchangeDeclare(exchange: BROKER_NAME,
                                    type: "direct");

            // connect to the queue
            channel.QueueDeclare(queue: _queueName,
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            //channel.QueueDeclare(queue: _queueNameForAzureFunction,
            //                        durable: true,
            //                        exclusive: false,
            //                        autoDelete: false,
            //                        arguments: null);

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");

                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel();
                StartBasicConsume();
            };

            return channel;
        }
        private void StartBasicConsume()
        {
            _logger.LogTrace("Starting RabbitMQ basic consume");

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
                _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
            }
        }
        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

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
                _logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", message);
            }

            // Even on exception we take the message off the queue.
            // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
            // For more information see: https://www.rabbitmq.com/dlx.html
            _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }
        private async Task ProcessEvent(string eventName, string message)
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                //using (var scope = _autofac.BeginLifetimeScope(AUTOFAC_SCOPE_NAME)) // can do it this way with autofac or like directly below with built in .net core DI 
                using (var scope = _serviceProvider.CreateScope())
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.IsDynamic)
                        {
                            var handler = scope.ServiceProvider.GetRequiredService(subscription.HandlerType) as IDynamicIntegrationEventHandler;
                            if (handler == null) continue;

                            using dynamic eventData = JsonDocument.Parse(message);
                            await handler.HandleAsync(eventData);
                        }
                        else
                        {
                            var handler = scope.ServiceProvider.GetRequiredService(subscription.HandlerType);
                            if (handler == null) continue;
                            var eventType = _subsManager.GetEventTypeByName(eventName);
                            var integrationEvent = JsonSerializer.Deserialize(message, eventType, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                            await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                        }
                    }
                }
            }
            else
            {
                _logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
            }
        }
        private void SubsManager_OnEventRemoved(object? sender, string eventName)
        {
            if (!_rabbitMQConnectionManagementService.IsConnected)
            {
                _rabbitMQConnectionManagementService.CreateModel();
            }

            using (var channel = _rabbitMQConnectionManagementService.CreateModel())
            {
                channel.QueueUnbind(queue: _queueName,
                    exchange: BROKER_NAME,
                    routingKey: eventName);

                if (_subsManager.IsEmpty)
                {
                    _queueName = string.Empty;
                    _consumerChannel.Close();
                }
            }
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEventBase
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();
            DoInternalSubscription(eventName);

            _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);

            _subsManager.AddSubscription<T, TH>();
            _rabbitMqBaseConsumer.StartBasicConsume();
        }

     

        public void Unsubscribe<T, TH>()
            where T : IntegrationEventBase
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();

            _logger.LogInformation("Unsubscribing from event {EventName}", eventName);

            _subsManager.RemoveSubscription<T, TH>();
        }
        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!_rabbitMQConnectionManagementService.IsConnected)
                {
                    _rabbitMQConnectionManagementService.CreateModel();
                }

                _consumerChannel.QueueBind(queue: _queueName,
                                    exchange: BROKER_NAME,
                                    routingKey: eventName); // routing key also known as 'Binding Key' // After run correlate this method with example online https://www.rabbitmq.com/tutorials/tutorial-four-dotnet.html
            }
        }
    }
}
