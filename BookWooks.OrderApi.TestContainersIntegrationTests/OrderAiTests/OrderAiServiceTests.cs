using BookWooks.OrderApi.Infrastructure.AiServices;
using BookWooks.OrderApi.Infrastructure.AiServices.Interfaces;
using BookWooks.OrderApi.TestContainersIntegrationTests.TestSetup;
using BookWooks.OrderApi.UseCases.Orders.AiServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BookWooks.OrderApi.TestContainersIntegrationTests.AiServices;

[Collection("Order Test Collection")]
public class OrderAiServiceTests : ApiTestBase<Program, BookyWooksOrderDbContext>
{
    private readonly ICustomerSupportService _customerSupportService;
    private readonly IProductSearchService _productSearchService;

    public OrderAiServiceTests(OrderWebApplicationFactory<Program> testFactory)
        : base(testFactory)
    {
        var scope = testFactory.Services.CreateScope();
        _customerSupportService = scope.ServiceProvider.GetRequiredService<ICustomerSupportService>();
        _productSearchService = scope.ServiceProvider.GetRequiredService<IProductSearchService>();
    }

    [Fact]
    public async Task CustomerSupportAsync_WithValidQuery_ReturnsResponse()
    {
        // Arrange
        var query = "How do I track my order?";

        // Act
        var result = await _customerSupportService.CustomerSupportAsync(query);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SearchProductsAsync_WithNumericCount_ReturnsCorrectNumberOfProducts()
    {
        // Arrange
        var query = "Find me 3 books about love";

        // Act
        var results = await _productSearchService.SearchProductsAsync(query);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task SearchProductsAsync_WithWordCount_ReturnsCorrectNumberOfProducts()
    {
        // Arrange
        var query = "Find me two books with a powerful narrative";

        // Act
        var results = await _productSearchService.SearchProductsAsync(query);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchProductsAsync_WithNoCount_ReturnsFallbackNumberOfProducts()
    {
        // Arrange
        var query = "Find me sci-fi books";

        // Act
        var results = await _productSearchService.SearchProductsAsync(query);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(5); // Default fallback count
    }

    //[Theory]
    //[InlineData("")]
    //[InlineData(null)]
    //public async Task CustomerSupportAsync_WithInvalidQuery_ReturnsEmptyOrErrorResponse(string query)
    //{
    //    // Act
    //    var result = await _customerSupportService.CustomerSupportAsync(query);

    //    // Assert
    //    result.Should().NotBeNull(); // Service should handle empty/null queries gracefully
    //}

    [Theory]
    [InlineData("couple", 2)]
    [InlineData("few", 3)]
    [InlineData("dozen", 12)]
    public async Task SearchProductsAsync_WithWordNumbers_ReturnsCorrectCount(string wordNumber, int expectedCount)
    {
        // Arrange
        var query = $"Find me a {wordNumber} of programming books";

        // Act
        var results = await _productSearchService.SearchProductsAsync(query);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(expectedCount);
    }
}