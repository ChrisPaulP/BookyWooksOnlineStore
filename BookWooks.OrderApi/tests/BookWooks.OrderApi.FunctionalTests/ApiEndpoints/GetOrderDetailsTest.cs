
using Ardalis.HttpClientTestExtensions;


using Xunit;
using BookWooks.OrderApi.Web.Orders;
using Microsoft.Extensions.Internal;
using Microsoft.AspNetCore.Http;


namespace BookWooks.OrderApi.FunctionalTests.ApiEndpoints;
[Collection("Sequential")]
public class GetOrderDetailsTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client;
  public GetOrderDetailsTest(CustomWebApplicationFactory<Program> factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task ReturnsSeedOrderGivenId()
  {
    Guid expectedOrderId = TestData.Order1.Id;

    //var response = await _client.GetAsync(GetOrderDetailsRequest.BuildRoute(expectedOrderId));
    //var content = await response.Content.ReadAsStringAsync();
    //Console.WriteLine(content); // Log or print the content

    var result = await _client.GetAndDeserializeAsync<GetOrderDetailsResponse>(GetOrderDetailsRequest.BuildRoute(expectedOrderId));

    Assert.Equal(expectedOrderId, result.Id);
    Assert.Equal(TestData.Order1.Status.Label, result.Status);
  }

  [Fact]
  public async Task ReturnsStatus404OrderNotFound()
  {
    Guid expectedOrderId = Guid.NewGuid();

    var result = await _client.GetAsync(GetOrderDetailsRequest.BuildRoute(expectedOrderId));
    
    Assert.Equal(StatusCodes.Status404NotFound, (int)result.StatusCode);

  }
  [Fact]
  public async Task ReturnsNotFoundGivenIdEmpty()
  {
    string route = GetOrderDetailsRequest.BuildRoute(Guid.Empty);
    _ = await _client.GetAndEnsureNotFoundAsync(route);
  }
}
