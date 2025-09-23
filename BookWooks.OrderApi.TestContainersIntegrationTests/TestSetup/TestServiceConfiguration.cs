using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.Qdrant;

namespace BookWooks.OrderApi.TestContainersIntegrationTests.TestSetup;

public class TestServiceConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration,
        QdrantContainer qdrantContainer)
    {
        ConfigureDbContext(services, configuration);
        ConfigureVectorStore(services, qdrantContainer);
    }

    private static void ConfigureDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<BookyWooksOrderDbContext>));

        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<BookyWooksOrderDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("OrderDatabase")));
    }

    private static void ConfigureVectorStore(IServiceCollection services, QdrantContainer qdrantContainer)
    {
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(VectorStore));

        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddSingleton<VectorStore>(sp =>
        {
            var qdrantClient = new QdrantClient(
                host: qdrantContainer.Hostname,
                port: qdrantContainer.GetMappedPublicPort(6334),
                apiKey: "your-secret-api-key-here"
            );

            return new QdrantVectorStore(qdrantClient, ownsClient: true);
        });
    }
}
