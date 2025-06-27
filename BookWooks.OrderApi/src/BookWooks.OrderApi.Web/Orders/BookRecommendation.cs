using BookWooks.OrderApi.UseCases.Orders.GetBookRecommendation;

namespace BookWooks.OrderApi.Web.Orders;

public class BookRecommendation : IEndpoint
{
  public void MapEndpoint(WebApplication app) => app
             .MapPost(BookRecommendationRequest.Route, HandleAsync);



  public static async Task<IResult> HandleAsync([FromBody] BookRecommendationRequest request, IMediator mediator, CancellationToken ct) =>

    (await mediator.Send(new GetBookRecommendationQuery(request.Genre)))
    .Match((bookRecommendation) => Results.Ok(bookRecommendation),
           (error) => Results.NotFound(error));
}

