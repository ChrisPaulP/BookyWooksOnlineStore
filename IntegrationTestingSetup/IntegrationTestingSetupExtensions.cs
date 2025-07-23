using DotNet.Testcontainers.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace IntegrationTestingSetup;

//public static class IntegrationTestingSetupExtensions
//{
//    public static MsSqlContainer CreateMsSqlContainer()
//    {
//        var MsSqlImage = "mcr.microsoft.com/mssql/server:2022-latest";
//        var MsSqlPassword = "yourStrong(!)Password";
//        var MsSqlPort = 1433;

//        return new MsSqlBuilder()
//            .WithImage(MsSqlImage)
//            .WithPortBinding(0, MsSqlPort)
//            .WithEnvironment("ACCEPT_EULA", "Y")
//            .WithEnvironment("SA_PASSWORD", MsSqlPassword)
//            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(MsSqlPort))
//            .Build();
//    }

//    public static RabbitMqContainer CreateRabbitMqContainer()
//    {
//        var RabbitMqImage = "rabbitmq:3-management-alpine";
//        var RabbitMqUsername = "guest";
//        var RabbitMqPassword = "guest";
//        var RabbitMqPort = 5672;

//        return new RabbitMqBuilder()
//            .WithImage(RabbitMqImage)
//            .WithPortBinding(5672, RabbitMqPort)
//            .WithEnvironment("RABBITMQ_DEFAULT_USER", RabbitMqUsername)
//            .WithEnvironment("RABBITMQ_DEFAULT_PASS", RabbitMqPassword)
//            .WithWaitStrategy(Wait.ForUnixContainer()
//                .UntilPortIsAvailable(RabbitMqPort)
//                .UntilPortIsAvailable(15672))
//            .Build();
//    }
//    public static RedisContainer CreateRedisContainer()
//    {
//        return new RedisBuilder()
//         .WithImage("redis:7-alpine")
//         .WithCleanUp(true)
//         .Build();
//    }


//    public static async Task StartContainersAsync(this MsSqlContainer msSqlContainer, RabbitMqContainer rabbitMqContainer, RedisContainer redisContainer) //, RedisContainer redisContainer)
//    {
//        await msSqlContainer.StartAsync();
//        await rabbitMqContainer.StartAsync();
//        await redisContainer.StartAsync();
//    }
//}
public static class IntegrationTestingSetupExtensions
{
    public static MsSqlContainer CreateMsSqlContainer()
    {
        return new MsSqlBuilder()
       .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
       .WithPortBinding(0, 1433)
       .WithEnvironment("ACCEPT_EULA", "Y")
       .WithEnvironment("SA_PASSWORD", "Your_password123")
       .WithWaitStrategy(
           Wait.ForUnixContainer()
               .UntilPortIsAvailable(1433)
               .AddCustomWaitStrategy(new WaitUntilSqlServerIsReady())
       )
       .WithCleanUp(true)
       .Build();
    }

    public static RabbitMqContainer CreateRabbitMqContainer()
    {
        return new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .WithPortBinding(0, 5672)
        .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
        .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
        .WithWaitStrategy(
            Wait.ForUnixContainer()
                .UntilPortIsAvailable(5672)
                .AddCustomWaitStrategy(new WaitUntilRabbitMqIsReady())
        )
        .WithCleanUp(true)
        .Build();
    }

    public static RedisContainer CreateRedisContainer()
    {
        return new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithPortBinding(0, 6379) // ✅ Explicit for CI
            .WithCleanUp(true)
            .Build();
    }

    public static async Task StartContainersAsync(
        this MsSqlContainer msSqlContainer,
        RabbitMqContainer rabbitMqContainer,
        RedisContainer redisContainer)
    {
        // ✅ Parallel startup for faster CI
        await Task.WhenAll(
            msSqlContainer.StartAsync(),
            rabbitMqContainer.StartAsync(),
            redisContainer.StartAsync()
        );
    }
}

