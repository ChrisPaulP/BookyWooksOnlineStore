using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using Testcontainers.Qdrant;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace BookWooks.OrderApi.TestContainersIntegrationTests.TestSetup;

public class TestContainerBuilder
{

    private readonly INetwork _network;
    private readonly IConfiguration _configuration;
    public TestContainerBuilder(INetwork network)
    {
        _network = network;
        // Loading secrets from JSON file
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddUserSecrets<TestContainerBuilder>()
            .Build();
    }
    public MsSqlContainer BuildSqlContainer() =>
        new MsSqlBuilder()
            .WithPassword(ContainerConfiguration.SqlPassword)
            .WithNetwork(_network)
            .WithNetworkAliases("sql-server")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(1433))
            .Build();

    public RabbitMqContainer BuildRabbitMqContainer() =>
        new RabbitMqBuilder()
          .WithName($"rabbitmq")
            .WithUsername(ContainerConfiguration.RabbitMqUsername)
            .WithPassword(ContainerConfiguration.RabbitMqPassword)
            .WithNetwork(_network)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5672))
            .Build();

    public RedisContainer BuildRedisContainer() =>
        new RedisBuilder()
            .WithImage("redis:7")
            .WithNetwork(_network)
            .WithPortBinding(6379, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(6379))
            .Build();

    public QdrantContainer BuildQdrantContainer() =>
        new QdrantBuilder()
            .WithImage("qdrant/qdrant:latest")
            //.WithName("qdrant-test")
            .WithPortBinding(6333, true)
            .WithPortBinding(6334, true)
            .WithNetwork(_network)
            .WithNetworkAliases("qdrant")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(6334))
            .Build();

    public IContainer BuildMcpServerContainer(QdrantContainer qdrantContainer)
    {
        const int MCP_PORT = 8181;
        var apiKey = GetOpenAIApiKey();
        var environmentVariables = new Dictionary<string, string>
        {
            ["ASPNETCORE_URLS"] = $"http://+:{MCP_PORT}",
            ["OpenAIOptions__OpenAiApiKey"] = apiKey,
            ["OpenAIOptions__EmbeddingModelId"] = "text-embedding-3-small",
            ["OpenAIOptions__ModelId"] = "gpt-4o-mini",
            ["MCP__BasePath"] = "/mcp",
            ["QdrantOptions__QdrantHost"] = qdrantContainer.Hostname,
            ["QdrantOptions__QdrantPort"] = qdrantContainer.GetMappedPublicPort(6334).ToString()
        };

        return new ContainerBuilder()
            .WithImage("bookwooks/mcpserver:latest")
            .WithPortBinding(MCP_PORT, true)
            .WithName("mcp-test-server")
            .WithHostname("mcp-test-server")
            .WithEnvironment(environmentVariables)
            .WithNetwork(_network)
            .WithWaitStrategy(
                Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(MCP_PORT)
                    .UntilMessageIsLogged("Application started"))
            .Build();
    }

    private string GetOpenAIApiKey()
    {
        var apiKey = Environment.GetEnvironmentVariable("OpenAIOptions__OpenAiApiKey") ?? _configuration["OpenAIOptions:OpenAiApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException(
                "Missing OpenAI API Key. Set either OpenAIOptions__OpenAiApiKey environment variable or OpenAIOptions:OpenAiApiKey in secrets.json");
        }

        return apiKey;
    }
}
