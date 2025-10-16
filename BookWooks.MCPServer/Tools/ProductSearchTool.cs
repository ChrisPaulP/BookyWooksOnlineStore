using BookWooks.MCPServer.Resources;
using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookWooks.OrderApi.UseCases.Products;
using BookyWooks.SharedKernel.Repositories;
using MCPServer;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using System.Collections;
using System.ComponentModel;
using System.Text.Json.Serialization;


public sealed class ProductSearchTool
{
    private readonly VectorStore _vectorStore;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

    public ProductSearchTool(VectorStore vectorStore, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        _vectorStore = vectorStore;
        _embeddingGenerator = embeddingGenerator;
    }
    public record ProductIdsSearchResult([property: JsonPropertyName("productIds")] IEnumerable<Guid> ProductIds);

    [KernelFunction, Description("Searches product embeddings and returns top product IDs")]
    public async Task<ProductIdsSearchResult> SearchAsync(
        [Description("Search prompt")] string prompt,
        [Description("Collection name in vector store")] string collection,
        [Description("Max number of results")] int topN = 10,
        CancellationToken cancellationToken = default)
    {
        var promptEmbedding = (await _embeddingGenerator.GenerateAsync(prompt, cancellationToken: cancellationToken)).Vector;

        var vsCollection =  _vectorStore.GetCollection<Guid, ProductVectorModel>(collection);

        // Return only IDs (not stale product details)
        var productIds = await vsCollection.SearchTopAsync(
            promptEmbedding,
            record => record.Id,
            topN,
            cancellationToken
        );

        return new ProductIdsSearchResult(productIds);
    }
}