using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace MCPServer;

public static class VectorStoreExtensions
{
    public static async Task<VectorStoreCollection<TKey, TModel>> GetOrCreateAndPopulateAsync<TKey, TModel>(
       this VectorStore vectorStore,
       string collection,
       Func<Task<IEnumerable<TModel>>> getData,
       Func<VectorStoreCollection<TKey, TModel>, TModel, Task> upsertAsync,
       CancellationToken cancellationToken = default)
       where TKey : notnull
       where TModel : class
    {
        var vsCollection = vectorStore.GetCollection<TKey, TModel>(collection);

        if (!await vsCollection.CollectionExistsAsync(cancellationToken))
        {
            await vsCollection.EnsureCollectionExistsAsync(cancellationToken);
            var data = await getData();
            foreach (var item in data)
            {
                await upsertAsync(vsCollection, item);
            }
        }
        return vsCollection;
    }

    public static async Task<List<TResult>> SearchTopAsync<TKey, TModel, TResult>(
        this VectorStoreCollection<TKey, TModel> collection,
        ReadOnlyMemory<float> embedding,
        Func<TModel, TResult> map,
        int top = 3,
        float minScore = 1f, // Add a minimum similarity threshold parameter
        CancellationToken cancellationToken = default)
        where TModel : class
        where TKey : notnull
    {
        var options = new VectorSearchOptions<TModel>();
        var results = collection.SearchAsync(embedding, top, options, cancellationToken);
        var list = new List<TResult>();
        await foreach (var result in results.WithCancellation(cancellationToken))
        {
            // Only include results above the threshold
            if (result.Score >= minScore)
            {
                list.Add(map(result.Record));
            }
        }
        return list;
    }

}
