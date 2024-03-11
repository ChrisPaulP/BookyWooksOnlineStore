
using DotNet.Testcontainers.Builders;
using Microsoft.Extensions.Logging;
using Testcontainers.RabbitMq;

namespace BookWooks.OrderApi.FunctionalTests;
public interface IRabbitMqContainerFactory
{
  RabbitMqContainer CreateContainer();
}
public class RabbitMqContainerFactory : IRabbitMqContainerFactory
{


  public HttpClient HttpClient { get; private set; } = default!;

  private const string RabbitMqUsername = "guest";
  private const string RabbitMqPassword = "guest";


  public RabbitMqContainer CreateContainer()
  {
    var rabbitMqContainer = new RabbitMqBuilder()
                          .WithImage("rabbitmq:3.11")
                           .WithPortBinding(5672, 5672)
            .WithEnvironment("RABBITMQ_DEFAULT_USER", RabbitMqUsername)
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", RabbitMqPassword)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672)) // Adding wait strategy
            .Build();
    return rabbitMqContainer;
  }
}
