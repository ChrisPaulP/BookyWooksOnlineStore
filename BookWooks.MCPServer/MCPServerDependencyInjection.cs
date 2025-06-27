#pragma warning disable SKEXP0110 
#pragma warning disable SKEXP0010
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;


namespace BookWooks.MCPServer;

public static class McpServerDependencyInjection
{
    public static IServiceCollection AddMcpServerServices(this IServiceCollection services)
    {
        RegisterMCPServer(services);
        RegisterKernel(services);
        RegisterVectorStore(services);
        RegisterEmbeddingGenerator(services);
        RegisterTools(services);


        return services;
    }

    private static void RegisterMCPServer(IServiceCollection services)
    {
        services
       .AddMcpServer()
       .WithStdioServerTransport()
       .WithTools(sp =>
       {
           var kernel = sp.GetRequiredService<Kernel>();
           return kernel;
       })
       .WithPrompt(PromptDefinition.Create(EmbeddedResource.ReadAsString("BookRecommendations.json")))
       .WithPrompt(PromptDefinition.Create(EmbeddedResource.ReadAsString("ReserveStock.json")));
    }

    private static void RegisterKernel(IServiceCollection services)
    {
        IKernelBuilder kernelBuilder = services.AddKernel();
        kernelBuilder.Plugins.AddFromType<OrderProcessingTool>();
        kernelBuilder.Plugins.AddFromType<BookRecommendationTool>();
        kernelBuilder.Plugins.AddFromType<ReservationTool>();
    }

    private static void RegisterVectorStore(IServiceCollection services)
    {
        services.AddSingleton<VectorStore>(sp =>
        {
            var qdrantClient = new QdrantClient(
                host: "localhost",
                port: 6333//,
               // apiKey: "your-qdrant-api-key"
            );
            return new QdrantVectorStore(qdrantClient, ownsClient: true);
        });
    }

    private static void RegisterEmbeddingGenerator(IServiceCollection services)
    {

        services.AddOpenAIEmbeddingGenerator("", "");
    }

    private static void RegisterTools(IServiceCollection services)
    {
        // Add additional tool registrations here if needed
    }
}
