

using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;


namespace IntegrationTestingSetup;

public class WaitUntilRabbitMqIsReady : IWaitUntil
{
    public async Task<bool> UntilAsync(IContainer container, CancellationToken ct = default)
    {
        try
        {
            var factory = new RabbitMQ.Client.ConnectionFactory
            {
                HostName = container.Hostname,
                Port = container.GetMappedPublicPort(5672),
                UserName = "guest",
                Password = "guest"
            };

            using var connection = factory.CreateConnection();
            return connection.IsOpen;
        }
        catch
        {
            return false; // retry until ready
        }
    }

    public Task<bool> UntilAsync(IContainer container)
        => UntilAsync(container, default);

}
