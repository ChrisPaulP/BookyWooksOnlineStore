using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using BookyWooks.MessageBus.IntegrationEventSetUp;
using System.Text.Json;
using BookyWooks.MessageBus.EventBusSubscriptionsManager;
using Microsoft.Extensions.Logging;

namespace BookyWooks.RabbitMqMessageBus;

public class RabbitMqBaseConsumer : IRabbitMqBaseConsumer
{
    private IModel _channel;
    private IRabbitMqConnectionSetUp _rabbitMqConnectionSetUp;
    private readonly IEventBusSubscriptionsManager _subsManager;
    private readonly ILogger<RabbitMqBaseConsumer> _logger;
    protected string QueueName { get; set; }

    protected RabbitMqBaseConsumer(string queueName, IRabbitMqConnectionSetUp rabbitMqConnectionSetUp, IEventBusSubscriptionsManager subsManager, ILogger<RabbitMqBaseConsumer> logger)
    {
        _subsManager = subsManager;
        _rabbitMqConnectionSetUp = rabbitMqConnectionSetUp;
        _logger = logger;
        _channel = _rabbitMqConnectionSetUp.CreateModel();
        _channel.QueueDeclare(queue: queueName, false, false, false, arguments: null);
        QueueName = queueName;
    }

    //protected  Task ExecuteAsync(CancellationToken stoppingToken)
    //{
    //    stoppingToken.ThrowIfCancellationRequested();

    //    var consumer = new EventingBasicConsumer(_channel);
    //    consumer.Received += async (ch, ea) =>
    //    {
    //        var content = Encoding.UTF8.GetString(ea.Body.ToArray());
    //        TMessage message = DeserializeMessage(content);
    //        await HandleMessage(message);

    //        _channel.BasicAck(ea.DeliveryTag, false);
    //    };
    //    _channel.BasicConsume(QueueName, false, consumer);

    //    return Task.CompletedTask;
    //}
    public void StartBasicConsume()
    {
        _logger.LogTrace("Starting RabbitMQ basic consume");

        if (_channel != null)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += Consumer_Received;
            _channel.BasicConsume(
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
        _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
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
    //protected abstract TMessage DeserializeMessage(string content);
    //protected abstract Task HandleMessage(TMessage message);
}

