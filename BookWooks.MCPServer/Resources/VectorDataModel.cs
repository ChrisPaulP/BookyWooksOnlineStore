using Microsoft.Extensions.VectorData;

namespace BookWooks.MCPServer.Resources;

/// A simple data model for a record in the vector store.
public class VectorDataModel
{
    [VectorStoreKey]
    public required Guid Key { get; init; }
    [VectorStoreData]
    public required string Text { get; init; }

    [VectorStoreVector(1536)]
    public required ReadOnlyMemory<float> Embedding { get; init; }
}
