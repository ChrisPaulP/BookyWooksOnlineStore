using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.VectorData;

namespace BookWooks.OrderApi.Infrastructure.AiServices.Models;
internal class ProductVectorModel
{
  [VectorStoreKey]
  public Guid Id { get; set; }
  [VectorStoreData]
  public string Name { get; set; } = default!;
  [VectorStoreData]
  public string Description { get; set; } = default!;
  [VectorStoreData]
  public double Price { get; set; }

  [NotMapped]
  [VectorStoreVector(1536)]
  public ReadOnlyMemory<float> Vector { get; set; }
}

