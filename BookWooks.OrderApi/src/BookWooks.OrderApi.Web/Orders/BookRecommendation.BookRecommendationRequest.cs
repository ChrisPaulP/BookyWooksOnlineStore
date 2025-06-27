namespace BookWooks.OrderApi.Web.Orders;

public record BookRecommendationRequest(string Genre) : IRequestWithRoute
{
  public static string Route => "/Orders/BookRecommendation/{Genre}";
}
