using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Ardalis.SharedKernel;
using BookWooks.OrderApi.Core.OrderAggregate;
using Newtonsoft.Json;


namespace BookWooks.OrderApi.UseCases.IntegrationEventHandlers;
//public class TestCaseTwoIntegrationEventHandler : BackgroundService
//{
//  private IModel _channel;
//  private IConnection _connection;
//  private readonly string _hostname;
//  private readonly string _queueName;
//  private readonly string _username;
//  private readonly string _password;
//  private readonly IOrderRepository _repository;


//  public TestCaseTwoIntegrationEventHandler(IOrderRepository repository, IOptions<RabbitMqConfiguration>
//      rabbitMqOptions)
//  {
//    _hostname = rabbitMqOptions.Value.EventHostName;
//    _queueName = rabbitMqOptions.Value.QueueName_PaymentDone;
//    _username = rabbitMqOptions.Value.EventBusUserName;
//    _password = rabbitMqOptions.Value.EventBusPassword;
//    var factory = new ConnectionFactory
//    {
//      HostName = _hostname,
//      UserName = _username,
//      Password = _password
//    };
//    _connection = factory.CreateConnection();
//    _channel = _connection.CreateModel();
//    _channel.QueueDeclare(queue: _queueName, durable: true,
//        exclusive: false,
//        autoDelete: false, arguments: null);
//    _repository = repository;
//  }
//  protected override Task ExecuteAsync(CancellationToken stoppingToken)
//  {
//    var consumer = new EventingBasicConsumer(_channel);
//    consumer.Received += (ch, ea) =>
//    {
//      var content = Encoding.UTF8.GetString(ea.Body.ToArray());
//      var paymentDone = JsonConvert.
//      DeserializeObject<PaymentOrderMessage>(content);
//      var resultHandeleMessage = HandleMessage(paymentDone);
//      if (resultHandeleMessage)
//        _channel.BasicAck(ea.DeliveryTag, false);

//    };

//    _channel.BasicConsume(_queueName, false, consumer);
//    return Task.CompletedTask;

//  }
//  private bool HandleMessage(PaymentOrderMessage? paymentOrderMessage)
//  {
//    if (paymentOrderMessage == null) return false;
//    var order = _repository.GetOrderByOrderId(paymentOrderMessage.OrderId);
//    if (order != null)
//    {
//      order.PaymentIsDone();
//      _repository.UnitOfWork.SaveChangesAsync();
//    }
//    return true;
//  }


//}

public class PaymentOrderMessage
{
  public Guid OrderId { get; set; }
}

