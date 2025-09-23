using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.OrderApi.Core.OrderAggregate.Specifications;

public class ProductsByIdsSpecification : BaseSpecification<Product>
{

  public ProductsByIdsSpecification(IEnumerable<Guid> productIds)
      : base(p => productIds.Select(id => ProductId.From(id)).Contains(p.ProductId))
  {
  }
}

