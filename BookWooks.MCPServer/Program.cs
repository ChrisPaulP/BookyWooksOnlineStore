#pragma warning disable SKEXP0010
//var builder = Host.CreateEmptyApplicationBuilder(settings: null);
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OpenAIOptions>(
    builder.Configuration.GetSection(OpenAIOptions.Key));

builder.Services.AddInfrastructureServicesForMCPServer(
    builder.Configuration,
    builder.Environment.IsDevelopment()
);


IKernelBuilder kernelBuilder = builder.Services.AddKernel();

kernelBuilder.Plugins.AddFromType<OrderProcessingTool>();
kernelBuilder.Plugins.AddFromType<BookRecommendationTool>();
kernelBuilder.Plugins.AddFromType<ReservationTool>();
kernelBuilder.Plugins.AddFromType<ProductSearchTool>();
kernelBuilder.Services.AddSingleton<VectorStore, InMemoryVectorStore>();
//builder.Services.AddSingleton<VectorStore>(sp =>
//{
//    var qdrantClient = new QdrantClient(
//        host: "localhost",      // Qdrant host (without http/https)
//        port: 6333//,             // Qdrant port
//                  //apiKey: "your-qdrant-api-key" // or null/empty if not required
//    );
//    return new QdrantVectorStore(qdrantClient, ownsClient: true);
//});
var openAiOptions = builder.Configuration
    .GetSection(OpenAIOptions.Key)
    .Get<OpenAIOptions>();
builder.Services.AddOpenAIEmbeddingGenerator(openAiOptions.EmbeddingModelId, openAiOptions.OpenAiApiKey);
//builder.WebHost.UseUrls("http://0.0.0.0:8181");
builder.Services
       .AddMcpServer()
       .WithHttpTransport()
       .WithTools(sp =>
       {
           var kernel = sp.GetRequiredService<Kernel>();
           return kernel;
       })
       .WithPrompt(PromptDefinition.Create(EmbeddedResource.ReadAsString("ReserveStock.json")))
        .WithResourceTemplate(VectorStoreSearchResourceTemplates.Create("customer-support.txt"));
       //.WithResourceTemplate(VectorStoreSearchResourceTemplates.Create("BookWooks.MCPServer.ProjectResources.customer-support.txt"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHealthChecks();

var host = builder.Build();
// Map the health check endpoint
host.MapHealthChecks("/health");
host.MapMcp();
host.UseCors();
host.Run();


