#pragma warning disable SKEXP0010
//var builder = Host.CreateEmptyApplicationBuilder(settings: null);

using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables(); // Ensure env vars are read
builder.Services.Configure<OpenAIOptions>(
    builder.Configuration.GetSection(OpenAIOptions.Key));

IKernelBuilder kernelBuilder = builder.Services.AddKernel();
kernelBuilder.Plugins.AddFromType<OrderProcessingTool>();
kernelBuilder.Plugins.AddFromType<BookRecommendationTool>();
kernelBuilder.Plugins.AddFromType<ReservationTool>();
kernelBuilder.Plugins.AddFromType<ProductSearchTool>();

//kernelBuilder.Services.AddSingleton<VectorStore, InMemoryVectorStore>();
builder.Services.AddSingleton<VectorStore>(sp =>
{
    var qdrantClient = new QdrantClient(
        host: "qdrant", //"localhost",      
        port: 6334//,             
       
    );
    return new QdrantVectorStore(qdrantClient, ownsClient: true);
});

var openAiOptions = builder.Configuration
    .GetSection(OpenAIOptions.Key)
    .Get<OpenAIOptions>();

if (openAiOptions is null || string.IsNullOrWhiteSpace(openAiOptions.OpenAiApiKey))
{
    Console.Error.WriteLine("ERROR: Missing OpenAI__OpenAiApiKey in configuration.");
    Environment.Exit(1); // fail fast with clear logs
}
builder.Services.AddOpenAIEmbeddingGenerator(openAiOptions.EmbeddingModelId, openAiOptions.OpenAiApiKey);

builder.Services
       .AddMcpServer()
       .WithHttpTransport()
       .WithResourceTemplate(VectorStoreSearchResourceTemplates.Create("customer-support.txt"))
       //.WithPrompt(PromptDefinition.Create(EmbeddedResource.ReadAsString("ReserveStock.json")))
       .WithTools(sp => sp.GetRequiredService<Kernel>());  
         

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
host.UseRouting();
host.UseCors();

host.MapHealthChecks("/health");
host.MapMcp();
host.Run();


