using BookWooks.OrderApi.UseCases.Products.GetProducts;

namespace BookWooks.OrderApi.Web.Products;

public class GetProducts : IEndpoint
{
  public void MapEndpoint(WebApplication app) => app
             .MapPost(GetProductsRequest.Route, HandleAsync)
             //.AddEndpointFilter<ValidationFilter<GetProductsRequest>>()
             .AllowAnonymous();

  public static async Task<IResult> HandleAsync([FromBody] GetProductsRequest request, IMediator mediator, CancellationToken ct) => (
           await mediator.Send(new GetProductsQuery(request.Prompt)))
          .Match(
           products => Results.Ok(products),
           productNotFound => Results.NotFound(new ProductNotFound()));
}
