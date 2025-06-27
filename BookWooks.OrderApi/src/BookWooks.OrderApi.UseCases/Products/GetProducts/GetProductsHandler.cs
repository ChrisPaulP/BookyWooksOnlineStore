using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.UseCases.Orders.AiServices;
using BookWooks.OrderApi.UseCases.Orders.Get;

namespace BookWooks.OrderApi.UseCases.Products.GetProducts;
internal class GetProductsHandler : IQueryHandler<GetProductsQuery, ProductSearchResult>
{
  private readonly IOrderAiService<ProductDto> _orderAiService;

    public GetProductsHandler(IOrderAiService<ProductDto> orderAiService) => _orderAiService = orderAiService;

  public async Task<ProductSearchResult> Handle(GetProductsQuery request, CancellationToken cancellationToken) =>
      (await _orderAiService.SearchProductsAsync(request.Prompt))
             .ToEither(() => new ProductNotFound());
}
