using Ardalis.HttpClientTestExtensions;
using BookWooks.OrderApi.Web;
using BookWooks.OrderApi.Web.Orders;
using Xunit;

namespace BookWooks.OrderApi.FunctionalTests.ApiEndpoints;
[Collection("Sequential")]
public class GetOrdersByStatus : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client;

  public GetOrdersByStatus(CustomWebApplicationFactory<Program> factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task ReturnsPendingOrders()
   {
    //var result = await _client.GetAndDeserializeAsync<GetOrderByStatusResponse>("/Contributors");
    var result = await _client.GetAndDeserializeAsync<GetOrderByStatusResponse>(GetOrderByStatusRequest.BuildRoute("Pending"));

    Assert.Single(result.Orders);
    Assert.Contains(result.Orders, o => o.Status == SeedData.Order1.Status.Name);
   // Assert.Contains(result.Orders, o => o.Status == SeedData.Order1.Status.Name);
  }
}
