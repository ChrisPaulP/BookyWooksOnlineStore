using BookyWooks.MessageBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookyWooks.RabbitMqMessageBus;

public class RabbitMqSendMessage : IMessageBus
{
    //TODO: move to appSettings file

    private const string ExchangeName = "PublishSubscribePaymentUpdate_Exchange";
    private readonly IRabbitMqConnectionSetUp _rabbitMqConnectionSetUp;
    private readonly ILogger<RabbitMqSendMessage> _logger;

    public RabbitMqSendMessage(IRabbitMqConnectionSetUp rabbitMqConnectionSetUp, ILogger<RabbitMqSendMessage> logger)
    {
        _rabbitMqConnectionSetUp = rabbitMqConnectionSetUp;
        _logger = logger;
    }

    public void SendMesage(BaseMessage message, string queueName)
    {
        if (_rabbitMqConnectionSetUp.IsConnected)
        {
            //create channels => Model to which a message is sent
            using var channel = _rabbitMqConnectionSetUp.CreateModel();
            //channel.QueueDeclare(queue: queueName, false, false, false, arguments: null);
            channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: false);
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);
            channel.BasicPublish(exchange: ExchangeName, "", basicProperties: null, body: body);
        }
    }

    public void Publish(IntegrationEvent @event)
    {
        if (!_rabbitMQConnectionManagementService.IsConnected)
        {
            _rabbitMQConnectionManagementService.TryConnect();
        }

        var policy = RetryPolicy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
            {
                _logger.LogWarning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id, $"{time.TotalSeconds:n1}", ex.Message);
            });

        var eventName = @event.GetType().Name;
        var isAzureFunction = false;
        if (eventName.Contains("AzureFunction"))
        {
            //_consumerChannel.QueueBind(queue: _queueNameForAzureFunction,
            //                   exchange: BROKER_NAME,
            //                   routingKey: eventName);
            isAzureFunction = true;
        }

        _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, eventName);

        using (var channel = _rabbitMQConnectionManagementService.CreateModel())
        {
            _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);

            channel.ExchangeDeclare(exchange: BROKER_NAME, type: "direct");

            var body = JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), new JsonSerializerOptions
            {
                WriteIndented = true
            });

            policy.Execute(() =>
            {
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent

                _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

                channel.BasicPublish(
                    exchange: isAzureFunction ? "" : BROKER_NAME,
                    routingKey: isAzureFunction ? _queueNameForAzureFunction : eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
            });
        }
    }
}
