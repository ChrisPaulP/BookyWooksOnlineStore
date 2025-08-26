using BookWooks.MCPServer.ProjectResources;
using MCPServer;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace BookWooks.MCPServer.Resources.Templates;

public static class VectorStoreSearchResourceTemplates
{
    public static ResourceTemplateDefinition Create(string embeddedResource, Kernel? kernel = null)
    {
        return new ResourceTemplateDefinition
        {
            Kernel = kernel,
            ResourceTemplate = new()
            {
                UriTemplate = "vectorStore://{collection}/{prompt}",
                Name = "Vector Store Record Retrieval",
                Description = "Retrieves relevant records from the vector store based on the provided prompt."
            },
            Handler = async (
                RequestContext<ReadResourceRequestParams> context,
                string collection,
                string prompt,
                [FromKernelServices] IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
                [FromKernelServices] VectorStore vectorStore,
                CancellationToken cancellationToken) =>
            {
                // Define how to get the data (lines from the file)
                async Task<IEnumerable<VectorDataModel>> GetData()
                {
                    string content = EmbeddedResource.ReadAsString(embeddedResource);
                    var lines = content.Split('\n');
                    var result = new List<VectorDataModel>();
                    foreach (var line in lines)
                    {
                        var embedding = (await embeddingGenerator.GenerateAsync(line, cancellationToken: cancellationToken)).Vector;
                        result.Add(new VectorDataModel
                        {
                            Key = Guid.NewGuid(),
                            Text = line,
                            Embedding = embedding
                        });
                    }
                    return result;
                }

                // Use the extension to get or create and populate the collection
                var vsCollection = await vectorStore.GetOrCreateAndPopulateAsync<Guid, VectorDataModel>(
                    collection,
                    GetData,
                    async (col, model) => await col.UpsertAsync(model, cancellationToken),
                    cancellationToken
                );

                // Generate embedding for the prompt
                ReadOnlyMemory<float> promptEmbedding = (await embeddingGenerator.GenerateAsync(prompt, cancellationToken: cancellationToken)).Vector;


                // Use the extension to search and map results
                var contents = await vsCollection.SearchTopAsync(
                    promptEmbedding,
                    record => (ResourceContents)new TextResourceContents
                    {
                        Text = record.Text,
                        Uri = context.Params!.Uri!,
                        MimeType = "text/plain"
                    },
                    3,
                     //0.3f, // Minimum similarity threshold
                    cancellationToken
                );

                return new ReadResourceResult { Contents = contents };
            }
        };
    }
}
