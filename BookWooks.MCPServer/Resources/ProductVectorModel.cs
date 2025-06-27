using Microsoft.Extensions.VectorData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.MCPServer.Resources;
    internal class ProductVectorModel
    {
    [VectorStoreKey]
    public Guid Id { get; set; }
    [VectorStoreData]
    public string Name { get; set; } = default!;
    [VectorStoreData]
    public string Description { get; set; } = default!;
    [VectorStoreData]
    public decimal Price { get; set; }

    [NotMapped]
    [VectorStoreVector(384)]
    public ReadOnlyMemory<float> Vector { get; set; }
}

