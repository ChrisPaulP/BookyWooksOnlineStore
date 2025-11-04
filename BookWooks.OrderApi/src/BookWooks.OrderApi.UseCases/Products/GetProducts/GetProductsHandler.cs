namespace BookWooks.OrderApi.UseCases.Products.GetProducts;
internal class GetProductsHandler : IQueryHandler<GetProductsQuery, ProductSearchResult>
{
  private readonly IProductSearchService _productSearchAiService;

    public GetProductsHandler(IProductSearchService productSearchAiService) => _productSearchAiService = productSearchAiService;

  public async Task<ProductSearchResult> Handle(GetProductsQuery request, CancellationToken cancellationToken) =>
      (await _productSearchAiService.SearchProductsAsync(request.Prompt))
             .ToEither<ProductErrors, IEnumerable<ProductDto>>(() => new ProductNotFound());
}
