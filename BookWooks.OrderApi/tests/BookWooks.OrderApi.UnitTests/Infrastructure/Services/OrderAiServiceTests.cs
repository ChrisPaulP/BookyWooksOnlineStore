using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookWooks.OrderApi.Core.OrderAggregate.Specifications;
using BookWooks.OrderApi.Infrastructure.AiMcpSetUp;
using BookWooks.OrderApi.Infrastructure.AiServices;
using BookWooks.OrderApi.Infrastructure.AiServices.Interfaces;
using BookWooks.OrderApi.UseCases.Products;
using BookyWooks.SharedKernel.Repositories;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Moq;
using OpenAI;
using OpenAI.Assistants;
using OpenAI.Responses;
using Xunit;

namespace BookWooks.OrderApi.UnitTests.Infrastructure.Services;

public class OrderAiServiceTests
{
  private readonly Mock<IAIAgentFactory> _aiAgentFactoryMock;
  private readonly Mock<IAiOperations> _aiOperationsMock;
  private readonly Mock<IReadRepository<Product>> _productRepositoryMock;
  private readonly Mock<IMcpClient> _mcpClientMock;
  private readonly Mock<AIAgent> _agentMock;
  private readonly OrderAiService _sut;

  public OrderAiServiceTests()
  {
    _aiAgentFactoryMock = new Mock<IAIAgentFactory>();
    _aiOperationsMock = new Mock<IAiOperations>();
    _productRepositoryMock = new Mock<IReadRepository<Product>>();
    _mcpClientMock = new Mock<IMcpClient>();
    _agentMock = new Mock<AIAgent>();

    _sut = new OrderAiService(
        _aiAgentFactoryMock.Object,
        _aiOperationsMock.Object,
        _productRepositoryMock.Object);
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
  public async Task CustomerSupportAsync_WithValidQuery_ReturnsExpectedResults()
  {
    // Arrange
    var query = "How do I track my order?";
    var expectedResponses = new List<AgentRunResponseUpdate>
    {
        new AgentRunResponseUpdate(
            ChatRole.Assistant,
            new List<AIContent>
            {
                new TextContent("Your order can be tracked...")
            })
        {
            MessageId = "1",
            ResponseId = "resp1"
        },
        new AgentRunResponseUpdate(
            ChatRole.Assistant,
            new List<AIContent>
            {
                new TextContent("Here's more info...")
            })
        {
            MessageId = "2",
            ResponseId = "resp1"
        }
    };
    var expectedResponse = "Your order can be tracked...Here's more info...";
    SetupCommonMocks(query, expectedResponses);

    // Act
    var result = await _sut.CustomerSupportAsync(query);

    // Assert
    Assert.Equal(expectedResponse, result);
    VerifyCommonMocks();
  }

  [Fact]
  public async Task SearchProductsAsync_WithValidQuery_ReturnsOrderedProducts()
  {
    // Arrange
    var query = "show me 2 fantasy books";
    var productIds = Enumerable.Range(0, 3).Select(_ => Guid.NewGuid()).ToList();
    var searchResult = new ProductIdsSearchResult { ProductIds = productIds };
    //var products = productIds.Select(id =>
    //    Product.CreateProduct(id, $"Book {id}", "Description", 9.99m, 10))
    //    .ToList();
    var products = new List<Product>
    {
        Product.CreateProduct(productIds[0], "Book 2", "Desc 2", 15.99m, 10).Match(
            product => product,
            errors => throw new InvalidOperationException($"Failed to create product: {string.Join(", ", errors)}")),
        Product.CreateProduct(productIds[1], "Book 1", "Desc 1", 10.99m, 10).Match(
            product => product,
            errors => throw new InvalidOperationException($"Failed to create product: {string.Join(", ", errors)}"))
    };

    SetupProductSearchMocks(query, searchResult, products);

    // Act
    var result = await _sut.SearchProductsAsync(query);

    // Assert
    Assert.Equal(2, result.Count());
    Assert.Equal(products.Select(p => p.ProductId.Value), result.Select(r => r.Id));
    VerifyProductSearchMocks();
  }

  [Theory]
  [InlineData("show me 5 books", 5)]
  [InlineData("i want three books", 3)]
  [InlineData("get me a dozen books", 12)]
  [InlineData("show me some books", 5)] // default fallback
  [InlineData("show me a couple of books", 2)]
  [InlineData("show me a few books", 3)]
  public void ExtractCount_WithVariousQueries_ReturnsExpectedCount(string query, int expectedCount)
  {
    var count = _sut.ExtractCount(query);
    Assert.Equal(expectedCount, count);
  }

  [Fact]
  public async Task CustomerSupportAsync_WhenResourceFails_ThrowsException()
  {
    // Arrange
    var query = "help with order";
    _aiAgentFactoryMock
        .Setup(x => x.CreateMcpClientAsync())
        .ReturnsAsync(_mcpClientMock.Object);

    _aiOperationsMock
        .Setup(x => x.GetResourceAsync(_mcpClientMock.Object, query))
        .ThrowsAsync(new Exception("Failed to get resource"));

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() => _sut.CustomerSupportAsync(query));
  }

  [Fact]
  public async Task SearchProductsAsync_WhenNoProductsFound_ReturnsEmptyList()
  {
    // Arrange
    var query = "show me 3 nonexistent books";
    var searchResult = new ProductIdsSearchResult { ProductIds = new List<Guid>() };
    SetupProductSearchMocks(query, searchResult, new List<Product>());

    // Act
    var result = await _sut.SearchProductsAsync(query);

    // Assert
    Assert.Empty(result);
  }

  private void SetupCommonMocks(string query, List<AgentRunResponseUpdate> expectedResponses)
  {
    _aiAgentFactoryMock
        .Setup(x => x.CreateMcpClientAsync())
        .ReturnsAsync(_mcpClientMock.Object);

    var resourceResult = new ReadResourceResult
    {
      Contents = new List<ResourceContents>
            {
                new TextResourceContents
                {
                    Text = "Order tracking information",
                    MimeType = "text/plain"
                }
            }
    };

    _aiOperationsMock
        .Setup(x => x.GetResourceAsync(_mcpClientMock.Object, query))
        .ReturnsAsync(resourceResult);

    _aiAgentFactoryMock
        .Setup(x => x.CreateAgent(It.IsAny<ChatClientAgentOptions>()))
        .Returns(_agentMock.Object);

    _agentMock
    .Setup(x => x.RunStreamingAsync(
        It.IsAny<IEnumerable<ChatMessage>>(),
        It.IsAny<AgentThread?>(),
        It.IsAny<AgentRunOptions?>(),
        It.IsAny<CancellationToken>()))
    .Returns(ToAsyncEnumerable(expectedResponses));

  }

  private void SetupProductSearchMocks(string query, ProductIdsSearchResult searchResult, List<Product> products)
  {
    _aiAgentFactoryMock
        .Setup(x => x.CreateMcpClientAsync())
        .ReturnsAsync(_mcpClientMock.Object);

    _mcpClientMock.Setup(x => x.SendRequestAsync(
    It.Is<JsonRpcRequest>(r => r.Method == RequestMethods.ToolsList),
    It.IsAny<CancellationToken>()))
    .ReturnsAsync(new JsonRpcResponse
    {
      Result = JsonSerializer.SerializeToNode(new { Tools = new List<McpClientTool>() })
    });

    _aiAgentFactoryMock
        .Setup(x => x.CreateAgent(It.IsAny<ChatClientAgentOptions>()))
        .Returns(_agentMock.Object);

    _agentMock
        .Setup(x => x.RunAsync(
            It.IsAny<IEnumerable<ChatMessage>>(),
            It.IsAny<AgentThread>(),
            It.IsAny<AgentRunOptions>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(new AgentRunResponse(
            new ChatMessage(ChatRole.Assistant, JsonSerializer.Serialize(searchResult))
        ));

    _productRepositoryMock
        .Setup(x => x.FindAllAsync(It.IsAny<ProductsByIdsSpecification>()))
        .ReturnsAsync(products);
  }
  private void VerifyCommonMocks()
  {
    _aiAgentFactoryMock.Verify(x => x.CreateMcpClientAsync(), Times.Once);
    _aiOperationsMock.Verify(x => x.GetResourceAsync(It.IsAny<IMcpClient>(), It.IsAny<string>()), Times.Once);
    _aiAgentFactoryMock.Verify(x => x.CreateAgent(It.IsAny<ChatClientAgentOptions>()), Times.Once);
  }

  private void VerifyProductSearchMocks()
  {
    _aiAgentFactoryMock.Verify(x => x.CreateMcpClientAsync(), Times.Once);
    _mcpClientMock.Verify(x => x.SendRequestAsync(It.Is<JsonRpcRequest>(r => r.Method == RequestMethods.ToolsList),It.IsAny<CancellationToken>()), Times.Once);
    _aiAgentFactoryMock.Verify(x => x.CreateAgent(It.IsAny<ChatClientAgentOptions>()), Times.Once);
    _productRepositoryMock.Verify(x => x.FindAllAsync(It.IsAny<ProductsByIdsSpecification>()), Times.Once);
  }
  private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> items)
  {
    foreach (var item in items)
    {
      yield return item;
      await Task.Yield(); // ensures true async behavior
    }
  }
}

