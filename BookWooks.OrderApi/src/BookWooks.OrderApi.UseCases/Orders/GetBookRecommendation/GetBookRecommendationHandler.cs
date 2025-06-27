//using BookyWooks.SharedKernel.AIInterfaces;

//namespace BookWooks.OrderApi.UseCases.Orders.GetBookRecommendation;
//internal class GetBookRecommendationHandler : IQueryHandler<GetBookRecommendationQuery, BookRecommendationResult>
//{
//    private readonly IAIClient _aiClient;
//    public GetBookRecommendationHandler(IAIClient aIClient) => _aiClient = aIClient;

//    public async Task<BookRecommendationResult> Handle(GetBookRecommendationQuery request, CancellationToken cancellationToken) =>
//        (await _aiClient.RunAsync(request.Genre))
//            .ToEither(() => "We do not have a recommendation for this genre.");
//}

