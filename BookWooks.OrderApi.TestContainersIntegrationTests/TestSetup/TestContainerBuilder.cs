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
    private readonly ContainerConfiguration _config;
    private readonly INetwork _network;
    private readonly IConfiguration _configuration;
    public TestContainerBuilder(ContainerConfiguration config, INetwork network)
    {
        _config = config;
        _network = network;
        // Loading secrets from JSON file
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("secrets.json", optional: true, reloadOnChange: true)
            .Build();
    }
    public MsSqlContainer BuildSqlContainer() =>
        new MsSqlBuilder()
            .WithPassword(ContainerConfiguration.SqlPassword)
            .WithNetwork(_network)
            .WithNetworkAliases("sql-server")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .Build();

    public RabbitMqContainer BuildRabbitMqContainer() =>
        new RabbitMqBuilder()
          .WithName($"rabbitmq")
            .WithUsername(ContainerConfiguration.RabbitMqUsername)
            .WithPassword(ContainerConfiguration.RabbitMqPassword)
            .WithNetwork(_network)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();

    public RedisContainer BuildRedisContainer() =>
        new RedisBuilder()
            .WithImage("redis:7")
            .WithNetwork(_network)
            .WithPortBinding(6379, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
            .Build();

    public QdrantContainer BuildQdrantContainer() =>
        new QdrantBuilder()
            .WithImage("qdrant/qdrant:latest")
            .WithName("qdrant-test")
            .WithPortBinding(6333, true)
            .WithPortBinding(6334, true)
            .WithNetwork(_network)
            .WithNetworkAliases("qdrant")
            //.WithBindMount(_config.QdrantStoragePath, "/qdrant/storage", AccessMode.ReadWrite)
            //.WithEnvironment("QDRANT_STORAGE_PATH", "/qdrant/storage")
            //.WithEnvironment("QDRANT_STORAGE__SNAPSHOTS_PATH", "/qdrant/storage/snapshots")
            //.WithEnvironment("QDRANT_STORAGE__SNAPSHOT_INTERVAL_SEC", "3600")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6334))
            .Build();

    //public IContainer BuildMcpServerContainer(QdrantContainer qdrantContainer) =>
    //    new ContainerBuilder()
    //        .WithImage("bookwooks/mcpserver:latest")
    //        //.WithImagePullPolicy(PullPolicy.Never)
    //        .WithPortBinding(8181, true)
    //        .WithName("mcp-test-server")
    //        .WithHostname("mcp-test-server")
    //        .WithEnvironment("ASPNETCORE_URLS", "http://+:8181")
    //          //.WithEnvironment("ASPNETCORE_DataProtection__Path", "/home/app/.aspnet/DataProtection-Keys")
    //          //.WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    //        .WithEnvironment("OpenAI__OpenAiApiKey", _configuration["OpenAI:OpenAiApiKey"])
    //        .WithEnvironment("OpenAI__EmbeddingModelId", "text-embedding-3-small")
    //        .WithEnvironment("OpenAI__ModelId", "gpt-4o-mini")
    //        .WithEnvironment("MCP__BasePath", "/mcp")
    //        //.WithEnvironment("ConnectionStrings__OrderDatabase", $"Server=sql-server;Database=BookyWooksOrderDbContext;User Id=sa;Password={ContainerConfiguration.SqlPassword};TrustServerCertificate=True")
    //        .WithEnvironment("QdrantOptions__QdrantHost", qdrantContainer.Hostname)
    //        .WithEnvironment("QdrantOptions__QdrantPort", qdrantContainer.GetMappedPublicPort(6334).ToString())
    //        //.WithBindMount(_config.DataProtectionPath, "/home/app/.aspnet/DataProtection-Keys")
    //        //.WithBindMount(_config.ProjectResourcesPath, "/app/ProjectResources", AccessMode.ReadWrite)
    //        //.WithExposedPort(8181)
    //        .WithNetwork(_network)
    //        .WithWaitStrategy(Wait.ForUnixContainer()
    //            .UntilPortIsAvailable(8181)
    //            .UntilMessageIsLogged("Application started"))
    //        .Build();
    public IContainer BuildMcpServerContainer(QdrantContainer qdrantContainer)
    {
        var apiKey = Environment.GetEnvironmentVariable("OpenAIOptions__OpenAiApiKey");

        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Missing OpenAIOptions__OpenAiApiKey in environment.");
        }
        //var apiKey = Environment.GetEnvironmentVariable("OpenAIOptions__OpenAiApiKey");
        return new ContainerBuilder()
            .WithImage("bookwooks/mcpserver:latest")
            .WithPortBinding(8181, true)
            .WithName("mcp-test-server")
            .WithHostname("mcp-test-server")
            .WithEnvironment("ASPNETCORE_URLS", "http://+:8181")
            .WithEnvironment("OpenAIOptions__OpenAiApiKey", apiKey) // inject secret
            //.WithEnvironment("OpenAI__OpenAiApiKey", _configuration["OpenAI:OpenAiApiKey"])
            .WithEnvironment("OpenAIOptions__EmbeddingModelId", "text-embedding-3-small")
            .WithEnvironment("OpenAIOptions__ModelId", "gpt-4o-mini")
            .WithEnvironment("MCP__BasePath", "/mcp")
            .WithEnvironment("QdrantOptions__QdrantHost", qdrantContainer.Hostname)
            .WithEnvironment("QdrantOptions__QdrantPort", qdrantContainer.GetMappedPublicPort(6334).ToString())
            .WithNetwork(_network)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(8181)
                .UntilMessageIsLogged("Application started"))
            .Build();
    }
}
