using Microsoft.SemanticKernel;
using System.ComponentModel;
using BookWooks.MCPServer.Resources;
using BookyWooks.SharedKernel.Repositories;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using MCPServer;
using BookWooks.OrderApi.UseCases.Products;


public sealed class ProductSearchTool
{
    private readonly VectorStore _vectorStore;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IReadRepository<Product> _productRepository;
    public ProductSearchTool(VectorStore vectorStore, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, IReadRepository<Product> productRepository)
    {
        _vectorStore = vectorStore;
        _embeddingGenerator = embeddingGenerator;
        _productRepository = productRepository;
    }
    [KernelFunction, Description("Searches products semantically by prompt")]
public async Task<IEnumerable<ProductDto>> SearchAsync(
    [Description("Search prompt")] string prompt,
    [Description("Collection name in vector store")] string collection,
    CancellationToken cancellationToken = default)
{
        var vsCollection = await _vectorStore.GetOrCreateAndPopulateAsync<Guid, ProductVectorModel>( 
        collection,
        async () =>
        {
            var repositoryProducts = await _productRepository.ListAllAsync();
            var vectors = new List<ProductVectorModel>();
            foreach (var product in repositoryProducts)
            {
                var productInfo = $"[{product.Name}] is a product that costs [{product.Price}] and is described as [{product.Description}]";

                var vector = (await _embeddingGenerator.GenerateAsync(productInfo, cancellationToken: cancellationToken)).Vector;
                vectors.Add(new ProductVectorModel
                {
                    Id = product.ProductId.Value,
                    Name = product.Name.Value,
                    Description = product.Description.Value,
                    Price = product.Price.Value,
                    Vector = vector
                });
            }
            return vectors;
        },
        async (collection, model) => await collection.UpsertAsync(model, cancellationToken),
        cancellationToken
    );

    var promptEmbedding = (await _embeddingGenerator.GenerateAsync(prompt, cancellationToken: cancellationToken)).Vector;

    return await vsCollection.SearchTopAsync(
        promptEmbedding,
        record => new ProductDto
        {
            Id = record.Id,
            Name = record.Name,
            Description = record.Description,
            Price = record.Price
        },
        1,
        0.3f, // Minimum similarity threshold
        cancellationToken
    );
}
}
