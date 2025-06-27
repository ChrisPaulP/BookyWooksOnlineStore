using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.OrderApi.UseCases.Products;
public static class ProductMappingExtensions
{
  public static ProductDto ToDto(this Product product)
  {
    if (product == null) throw new ArgumentNullException(nameof(product));
    return new ProductDto
    {
      Id = product.ProductId.Value,
      Name = product.Name.Value,
      Description = product.Description.Value,
      Price = product.Price.Value
    };
  }
  public static List<ProductDto> ToDtoList(this IEnumerable<Product> products)
  {
    if (products == null) throw new ArgumentNullException(nameof(products));
    return products.Select(p => p.ToDto()).ToList();
  }
}
