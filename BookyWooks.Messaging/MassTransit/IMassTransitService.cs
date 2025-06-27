namespace BookyWooks.Messaging.MassTransit;

public interface IMassTransitService
{
    Task Publish<T>(T message) where T : class;
    Task Send<T>(T message, string queuename) where T : class;
}
