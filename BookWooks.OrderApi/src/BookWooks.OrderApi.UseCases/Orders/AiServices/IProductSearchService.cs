using BookWooks.OrderApi.UseCases.Products;

namespace BookWooks.OrderApi.UseCases.Orders.AiServices;
public interface IProductSearchService
{
  public Task<IEnumerable<ProductDto>> SearchProductsAsync(string query);
}
