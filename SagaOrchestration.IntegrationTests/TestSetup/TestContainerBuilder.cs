using BookWooks.OrderApi.TestContainersIntegrationTests.TestSetup;
using DotNet.Testcontainers.Networks;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace SagaOrchestration.IntegrationTests;

public class TestContainerBuilder
{
    private readonly INetwork _network;

    public TestContainerBuilder(INetwork network)
    {
        _network = network;
    }

    public MsSqlContainer BuildSqlContainer()
    {
        return new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithNetwork(_network)
            .WithPassword(ContainerConfiguration.SqlPassword)
            .Build();
    }

    public RabbitMqContainer BuildRabbitMqContainer()
    {
        return new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management-alpine")
            .WithNetwork(_network)
            .WithUsername(ContainerConfiguration.RabbitMqUsername)
            .WithPassword(ContainerConfiguration.RabbitMqPassword)
            .Build();
    }
}