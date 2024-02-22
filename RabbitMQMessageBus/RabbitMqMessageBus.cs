using BookyWooks.MessageBus;
using BookyWooks.MessageBus.IntegrationEventSetUp;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace RabbitMQMessageBus;

public class RabbitMqMessageBus : IMessageBus
{
    private readonly string _hostname;
    private readonly string _password;
    private readonly string _username;
    private IConnection _connection;

    public RabbitMqMessageBus(IOptions<RabbitMqConfiguration> rabbitMqOptions)
    {

        _hostname = rabbitMqOptions.Value.EventHostName;
        _username = rabbitMqOptions.Value.EventBusUserName;
        _password = rabbitMqOptions.Value.EventBusPassword;

        CreateRabbitMQConnection();

    }

    public void SendMessage(IntegrationEventBase message, string exchange, string queueName = "")
    {
        if (CheckRabbitMQConnection())
        {
            using (var channel = _connection.CreateModel())
            {

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);
                var Properties = channel.CreateBasicProperties();
                Properties.Persistent = true;

                if (!string.IsNullOrEmpty(queueName))
                {
                    channel.QueueDeclare(queue: queueName,
                  durable: true, exclusive: false, autoDelete: false,
                  arguments: null);
                    channel.BasicPublish(exchange: "", routingKey: queueName
                        , basicProperties: Properties, body: body);
                }
                else
                {

                    channel.ExchangeDeclare(exchange, ExchangeType.Fanout, true, false, null);

                    channel.BasicPublish(exchange: exchange, routingKey: "", basicProperties: Properties, body: body);
                }


            }
        }
    }
    private void CreateRabbitMQConnection()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };
            _connection = factory.CreateConnection();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"can not create connection: {ex.Message}");
        }
    }

    private bool CheckRabbitMQConnection()
    {
        if (_connection != null)
        {
            return true;
        }
        CreateRabbitMQConnection();
        return _connection != null;
    }
}

}
