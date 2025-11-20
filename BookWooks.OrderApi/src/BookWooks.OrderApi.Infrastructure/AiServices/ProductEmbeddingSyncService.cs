using BookWooks.OrderApi.Infrastructure.AiServices.Models;
namespace BookWooks.OrderApi.Infrastructure.AiServices;
public class ProductEmbeddingSyncService
{
  private readonly Microsoft.Extensions.VectorData.VectorStore _vectorStore;
  private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
  private readonly IReadRepository<Product> _productRepository;
  private readonly ILogger<ProductEmbeddingSyncService> _logger;
  public ProductEmbeddingSyncService(Microsoft.Extensions.VectorData.VectorStore vectorStore,
                           IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
                           IReadRepository<Product> productRepository,
                           ILogger<ProductEmbeddingSyncService> logger)
  {
    _vectorStore = vectorStore;
    _embeddingGenerator = embeddingGenerator;
    _productRepository = productRepository;
    _logger = logger;
  }

  public async Task PopulateAsync(string collectionName, CancellationToken cancellationToken = default)
  {
    try
    {
      _logger.LogInformation("Starting product embedding sync for collection: {CollectionName}", collectionName);

      var products = await _productRepository.ListAllAsync();
      _logger.LogInformation("Retrieved {Count} products from repository", products.Count);

      try
      {
        _logger.LogDebug("Attempting to get vector collection: {CollectionName}", collectionName);
        var productVectorCollection = _vectorStore.GetCollection<Guid, ProductVectorModel>(collectionName);
        _logger.LogInformation("Successfully got vector collection");

        try
        {
          var exists = await productVectorCollection.CollectionExistsAsync();
          _logger.LogInformation("Collection exists check returned: {Exists}", exists);

          if (!exists)
          {
            _logger.LogInformation("Creating new collection: {CollectionName}", collectionName);
            await productVectorCollection.EnsureCollectionExistsAsync();
            _logger.LogInformation("Successfully created collection");
          }

          var processedCount = 0;
          var errorCount = 0;

          foreach (var product in products)
          {
            try
            {
              var productInfo = $"[{product.Name}] costs [{product.Price}] and is described as [{product.Description}]";
              _logger.LogDebug("Processing product: {ProductId} - {ProductInfo}", product.ProductId.Value, productInfo);

              var embedding = await _embeddingGenerator.GenerateAsync(productInfo, cancellationToken: cancellationToken);
              _logger.LogDebug("Generated embedding for product: {ProductId}", product.ProductId.Value);

              var vectorModel = new ProductVectorModel
              {
                Id = product.ProductId.Value,
                Name = product.Name.Value,
                Description = product.Description.Value,
                Price = (double)product.Price.Value,
                Vector = embedding.Vector
              };

              await productVectorCollection.UpsertAsync(vectorModel, cancellationToken);
              processedCount++;
              _logger.LogDebug("Successfully upserted vector for product: {ProductId}", product.ProductId.Value);
            }
            catch (Exception ex)
            {
              errorCount++;
              _logger.LogError(ex, "Error processing product {ProductId}: {Error}",
                  product.ProductId.Value, ex.Message);
            }
          }

          _logger.LogInformation(
              "Completed product embedding sync. Processed: {ProcessedCount}, Errors: {ErrorCount}",
              processedCount, errorCount);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error managing collection: {Error}", ex.Message);
          throw new InvalidOperationException($"Failed to manage collection {collectionName}", ex);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting vector collection: {Error}", ex.Message);
        throw new InvalidOperationException($"Failed to get vector collection {collectionName}", ex);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Fatal error during product embedding sync: {Error}", ex.Message);
      throw;
    }
  }
}
