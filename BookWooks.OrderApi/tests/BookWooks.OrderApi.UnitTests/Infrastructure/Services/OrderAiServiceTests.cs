using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookWooks.OrderApi.Core.OrderAggregate.Specifications;
using BookWooks.OrderApi.Infrastructure.AiMcpSetUp;
using BookWooks.OrderApi.Infrastructure.AiServices;
using BookWooks.OrderApi.Infrastructure.AiServices.Interfaces;
using BookWooks.OrderApi.UseCases.Products;
using BookyWooks.SharedKernel.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Moq;
using NSubstitute;
using Xunit;

namespace BookWooks.OrderApi.UnitTests.Infrastructure.Services;
public class OrderAiServiceTests
{
  private readonly Mock<IMcpFactory> _mcpFactoryMock;
  private readonly Mock<IMcpClient> _mcpClientMock;
  private readonly Mock<IChatCompletionService> _chatCompletionServiceMock;
  private readonly Mock<IAiOperations> _aiOperationsMock;
  private readonly OrderAiService _sut;
  private readonly Kernel _kernel;
  private readonly Mock<IReadRepository<Product>> _productRepositoryMock;

  public OrderAiServiceTests()
  {
    _mcpFactoryMock = new Mock<IMcpFactory>();
    _mcpClientMock = new Mock<IMcpClient>();
    _chatCompletionServiceMock = new Mock<IChatCompletionService>();
    _aiOperationsMock = new Mock<IAiOperations>();
    _productRepositoryMock = new Mock<IReadRepository<Product>>();
    // Create a real Kernel instance for the context
    var kernelBuilder = Kernel.CreateBuilder();

    // Setup chat completion service
    kernelBuilder.Services.AddSingleton<IChatCompletionService>(_chatCompletionServiceMock.Object);
    _kernel = kernelBuilder.Build();

    // Create McpContext with real kernel and mocked client
    var mcpContext = new McpContext(_mcpClientMock.Object, _kernel);

    _mcpFactoryMock
    .Setup(x => x.CreateClientAndKernelAsync())
    .ReturnsAsync(mcpContext);

    _sut = new OrderAiService(_mcpFactoryMock.Object, _aiOperationsMock.Object, _productRepositoryMock.Object);
  }

  [Theory]
  [InlineData("show me 5 books", 5)]
  [InlineData("i want three books", 3)]
  [InlineData("get me a dozen books", 12)]
  [InlineData("show me some books", 5)]
  [InlineData("show me a couple of books", 2)]
  [InlineData("show me a few books", 3)]
  public void ExtractCount_ShouldReturnCorrectCount(string query, int expected)
  {
    // Act
    var result = _sut.ExtractCount(query);

    // Assert
    Assert.Equal(expected, result);
  }

  [Fact]
  public async Task SearchProductsAsync_ShouldReturnOrderedProducts()
  {
    // Arrange
    var productIds = new List<Guid>
    {
        Guid.NewGuid(),
        Guid.NewGuid()
    };

    // Setup kernel response with product IDs
    var jsonResponse = new { content = new[] { new { text = JsonSerializer.Serialize(productIds) } } };
    var jsonElement = JsonSerializer.SerializeToElement(jsonResponse);

    // Create the function that will return our test data
    var functionDefinition = KernelFunctionFactory.CreateFromMethod(
        (Kernel kernel, KernelArguments arguments) =>
        {
          // Create FunctionResult with both the function and result data
          var currentFunction = kernel.Plugins
              .GetFunction(AiServiceConstants.ToolsPluginName, "ProductSearchTool_Search");
          return new FunctionResult(currentFunction, jsonElement);
        },
        "ProductSearchTool_Search");

    // Create and register the plugin
    var plugin = KernelPluginFactory.CreateFromFunctions(
        AiServiceConstants.ToolsPluginName,
        new[] { functionDefinition });
    _kernel.Plugins.Add(plugin);

    // Setup repository to return products in a different order
    // Create products list
    var products = new List<Product>
    {
        Product.CreateProduct(productIds[1], "Book 2", "Desc 2", 15.99m, 10).Match(
            product => product,
            errors => throw new InvalidOperationException($"Failed to create product: {string.Join(", ", errors)}")),
        Product.CreateProduct(productIds[0], "Book 1", "Desc 1", 10.99m, 10).Match(
            product => product,
            errors => throw new InvalidOperationException($"Failed to create product: {string.Join(", ", errors)}"))
    };

    _productRepositoryMock
        .Setup(r => r.FindAllAsync(It.IsAny<ProductsByIdsSpecification>()))
        .ReturnsAsync(products);

    var mcpContext = new McpContext(_mcpClientMock.Object, _kernel);
    _mcpFactoryMock.Setup(x => x.CreateClientAndKernelAsync())
        .ReturnsAsync(mcpContext);

    // Act
    var result = await _sut.SearchProductsAsync("show me two books");

    // Assert
    var resultList = result.ToList();
    Assert.NotNull(resultList);
    Assert.Equal(2, resultList.Count);

    // Verify order matches original productIds order
    Assert.Equal(productIds[0], resultList[0].Id);
    Assert.Equal(productIds[1], resultList[1].Id);

    // Verify product details
    Assert.Equal("Book 1", resultList[0].Name);
    Assert.Equal(10.99m, resultList[0].Price);
    Assert.Equal("Book 2", resultList[1].Name);
    Assert.Equal(15.99m, resultList[1].Price);

    // Verify repository was called with correct specification
    //_productRepositoryMock.Verify(r =>
    //    r.FindAllAsync(It.Is<ProductsByIdsSpecification>(s =>
    //        s.Criteria.ToString().Contains(productIds[0].ToString()) &&
    //        s.Criteria.ToString().Contains(productIds[1].ToString()))),
    //    Times.Once);
  }
  [Fact]
  public async Task SearchProductsAsync_WhenDeserializationFails_ShouldReturnEmptyList()
  {

    var noProducts = new List<ProductDto>();

    var functionDelegate = (Kernel kernel) =>
    {
      return new FunctionResult(kernel.Plugins.GetFunction(AiServiceConstants.ToolsPluginName, "ProductSearchTool_Search"), noProducts);
    };


    // Create a function first
    var function = KernelFunctionFactory.CreateFromMethod(
        functionDelegate,
        "ProductSearchTool_Search");

    // Add the function to the kernel's plugin collection
    var plugin = KernelPluginFactory.CreateFromFunctions(
        AiServiceConstants.ToolsPluginName,
        new[] { function });

    _kernel.Plugins.Add(plugin);

    // Act
    var result = await _sut.SearchProductsAsync("show me two books");

    // Assert
    Assert.NotNull(result);
    Assert.Empty(result);
  }
  [Fact]
  public async Task CustomerSupportAsync_ShouldReturnResponse()
  {
    // Arrange
    var query = "help with my order";
    var expectedResponse = "Here's how I can help with your order...";

    // Get the same context instance that was setup in the constructor
    var mcpContext = await _mcpFactoryMock.Object.CreateClientAndKernelAsync();

    // Mock the resource result
    var contents = new List<ResourceContents>
    {
        new TextResourceContents
        {
            Text = "some content",
            MimeType = "text/plain",
            Uri = "test://uri"
        }
    };
    var readResourceResult = new ReadResourceResult { Contents = contents };

    _aiOperationsMock
        .Setup(x => x.GetResourceAsync(mcpContext, query))
        .ReturnsAsync(readResourceResult);

    // Setup the chat completion response
    _aiOperationsMock
        .Setup(x => x.GetCompletionAsync(
            mcpContext,
            It.Is<ChatHistory>(h =>
                h.Count == 2 && // Verifies both messages were added
                h[1].Content == query))) // Verifies the query was added as second message
        .ReturnsAsync(expectedResponse);

    // Act
    var result = await _sut.CustomerSupportAsync(query);

    // Assert
    Assert.Equal(expectedResponse, result);

    // Verify all interactions
    _mcpFactoryMock.Verify(x => x.CreateClientAndKernelAsync(), Times.Exactly(2));

    _aiOperationsMock.Verify(
        x => x.GetResourceAsync(mcpContext, query),
        Times.Once);

    _aiOperationsMock.Verify(
        x => x.GetCompletionAsync(
            mcpContext,
            It.Is<ChatHistory>(h =>
                h.Count == 2 &&
                h[1].Content == query)),
        Times.Once);
  }
  [Fact]
  public async Task CustomerSupportAsync_WhenResourceFails_ShouldThrowException()
  {
    // Arrange
    var query = "help with my order";
    var mcpContext = new McpContext(_mcpClientMock.Object, _kernel);

    _mcpFactoryMock
        .Setup(x => x.CreateClientAndKernelAsync())
        .ReturnsAsync(mcpContext);

    _aiOperationsMock
        .Setup(x => x.GetResourceAsync(mcpContext, query))
        .ThrowsAsync(new InvalidOperationException("Failed to get resource"));

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => _sut.CustomerSupportAsync(query));
  }

  [Fact]
  public async Task CustomerSupportAsync_WhenCompletionFails_ShouldThrowException()
  {
    // Arrange
    var query = "help with my order";
    var mcpContext = new McpContext(_mcpClientMock.Object, _kernel);

    var contents = new List<ResourceContents>
    {
        new TextResourceContents
        {
            Text = "some content",
            MimeType = "text/plain",
            Uri = "test://uri"
        }
    };
    var readResourceResult = new ReadResourceResult { Contents = contents };

    _mcpFactoryMock
        .Setup(x => x.CreateClientAndKernelAsync())
        .ReturnsAsync(mcpContext);

    _aiOperationsMock
        .Setup(x => x.GetResourceAsync(mcpContext, query))
        .ReturnsAsync(readResourceResult);

    _aiOperationsMock
        .Setup(x => x.GetCompletionAsync(
            mcpContext,
            It.IsAny<ChatHistory>()))
        .ThrowsAsync(new InvalidOperationException("Failed to get completion"));

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => _sut.CustomerSupportAsync(query));
  }



}
