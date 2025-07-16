using BookWooks.OrderApi.Infrastructure.Common.Behaviour;
using BookWooks.OrderApi.UseCases.Errors;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static BookWooks.OrderApi.UseCases.Errors.DatabaseErrors;

public record OrderTestQuery() : IRequest<Either<OrderErrors, string>>;
public record CreateOrderTestQuery() : IRequest<Either<CreateOrderErrors, string>>;

public record NonEitherRequest : IRequest<string>;

public class ErrorHandlingBehaviorTests
{
  private readonly Mock<ILogger<ErrorHandlingBehavior<OrderTestQuery, Either<OrderErrors, string>>>> _mockLogger;

  public ErrorHandlingBehaviorTests()
  {
    _mockLogger = new Mock<ILogger<ErrorHandlingBehavior<OrderTestQuery, Either<OrderErrors, string>>>>();
  }

  [Fact]
  public async Task Returns_Next_Result_When_No_Orders_Exception()
  {
    // Arrange
    var expected = Prelude.Right<OrderErrors, string>("Success");
    var behavior = new ErrorHandlingBehavior<OrderTestQuery, Either<OrderErrors, string>>(_mockLogger.Object);

    RequestHandlerDelegate<Either<OrderErrors, string>> next = () => Task.FromResult(expected);

    // Act
    var result = await behavior.Handle(new OrderTestQuery(), next, CancellationToken.None);

    // Assert
    Assert.Equal(expected, result);
  }
  [Fact]
  public async Task Returns_Next_Result_When_No_CreateOrder_Exception()
  {
    // Arrange
    var logger = new Mock<ILogger<ErrorHandlingBehavior<CreateOrderTestQuery, Either<CreateOrderErrors, string>>>>();
    var expected = Prelude.Right<CreateOrderErrors, string>("Success");
    var behavior = new ErrorHandlingBehavior<CreateOrderTestQuery, Either<CreateOrderErrors, string>>(logger.Object);

    RequestHandlerDelegate<Either<CreateOrderErrors, string>> next = () => Task.FromResult(expected);

    // Act
    var result = await behavior.Handle(new CreateOrderTestQuery(), next, CancellationToken.None);

    // Assert
    Assert.Equal(expected, result);
  }
  [Fact]
  public async Task Wraps_Order_Exception_As_UnexpectedError_And_Logs()
  {
    // Arrange
    var behavior = new ErrorHandlingBehavior<OrderTestQuery, Either<OrderErrors, string>>(_mockLogger.Object);

    RequestHandlerDelegate<Either<OrderErrors, string>> next = () =>
        throw new InvalidOperationException("Something went wrong");

    // Act
    var result = await behavior.Handle(new OrderTestQuery(), next, CancellationToken.None);

    // Assert
    Assert.True(result.IsLeft);

    var left = result.LeftToList()[0];

    Assert.IsType<OrderErrors>(left);

    var networkError = Assert.IsType<NetworkErrors>(left.Value);

    Assert.IsType<UnexpectedError>(networkError.Value);

    _mockLogger.Verify(
        l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((_, __) => true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }
  [Fact]
  public async Task Wraps_Order_DbUpdateException_As_DatabaseError()
  {
    var behavior = new ErrorHandlingBehavior<OrderTestQuery, Either<OrderErrors, string>>(_mockLogger.Object);

    RequestHandlerDelegate<Either<OrderErrors, string>> next = () => throw new DbUpdateException("DB error", new Exception());

    var result = await behavior.Handle(new OrderTestQuery(), next, CancellationToken.None);

    Assert.True(result.IsLeft);
    var left = result.LeftToList()[0];

    Assert.IsType<OrderErrors>(left);

    var networkError = Assert.IsType<NetworkErrors>(left.Value);

    Assert.IsType<DatabaseError>(networkError.Value);

  }
  [Fact]
  public async Task Wraps_Order_Unknown_Exception_As_UnexpectedError()
  {
    var behavior = new ErrorHandlingBehavior<OrderTestQuery, Either<OrderErrors, string>>(_mockLogger.Object);
    RequestHandlerDelegate<Either<OrderErrors, string>> next = () => throw new InvalidCastException("weird cast");

    var result = await behavior.Handle(new OrderTestQuery(), next, CancellationToken.None);

    Assert.True(result.IsLeft);
    var left = result.LeftToList()[0];
    Assert.IsType<OrderErrors>(left);
    var unexpectedError = Assert.IsType<NetworkErrors>(left.Value);
    Assert.IsType<UnexpectedError>(unexpectedError.Value);
  }
  [Fact]
  public async Task Throws_When_Response_Type_Is_Not_Either()
  {
    // Arrange
    var logger = new Mock<ILogger<ErrorHandlingBehavior<NonEitherRequest, string>>>();
    var behavior = new ErrorHandlingBehavior<NonEitherRequest, string>(logger.Object);

    RequestHandlerDelegate<string> next = () => throw new System.Exception("fail");

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(() =>
        behavior.Handle(new NonEitherRequest(), next, CancellationToken.None));
  }
  [Fact]
  public async Task Wraps_CreateOrder_DbUpdateException_As_DatabaseError()
  {
    var logger = new Mock<ILogger<ErrorHandlingBehavior<CreateOrderTestQuery, Either<CreateOrderErrors, string>>>>();
    var behavior = new ErrorHandlingBehavior<CreateOrderTestQuery, Either<CreateOrderErrors, string>>(logger.Object);
    RequestHandlerDelegate<Either<CreateOrderErrors, string>> next = () => throw new DbUpdateException("DB error", new Exception());

    var result = await behavior.Handle(new CreateOrderTestQuery(), next, CancellationToken.None);

    Assert.True(result.IsLeft);
    var left = result.LeftToList()[0];

    // Check the outer type
    Assert.IsType<CreateOrderErrors>(left);

    // Check the inner type
    var networkError = Assert.IsType<NetworkErrors>(left.Value);

    // Check the wrapped error type
    Assert.IsType<DatabaseError>(networkError.Value);


  }

}
