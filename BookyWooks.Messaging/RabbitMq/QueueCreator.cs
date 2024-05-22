

namespace BookyWooks.Messaging.RabbitMq;

using BookyWooks.Messaging.Constants;
using RabbitMQ.Client;

public class QueueCreator
{
    private readonly string _rabbitMqConnectionString;

    public QueueCreator(string rabbitMqConnectionString)
    {
        _rabbitMqConnectionString = rabbitMqConnectionString;
    }

//    public void CreateQueues()
//    {
//        using (var connection = new ConnectionFactory { Uri = new Uri(_rabbitMqConnectionString) }.CreateConnection())
//        using (var channel = connection.CreateModel())
//        {
//            // Declare queues
//            DeclareQueue(channel, QueueConstants.OrderCreatedEventQueueName);
//            DeclareQueue(channel, QueueConstants.OrderCompletedEventQueueName);
//            DeclareQueue(channel, QueueConstants.OrderFailedEventQueueName);
//            DeclareQueue(channel, QueueConstants.UserCreatedEventQueueName);
//            DeclareQueue(channel, QueueConstants.CreateOrderMessageQueueName);
//            DeclareQueue(channel, QueueConstants.CompletePaymentCommandQueueName);
//            DeclareQueue(channel, QueueConstants.StockRollBackMessageQueueName);
//            DeclareQueue(channel, QueueConstants.CheckBookStockCommandQueueName);

//}
//    }

    private void DeclareQueue(IModel channel, string queueName)
    {
        channel.QueueDeclare(queue: queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
    }
}

