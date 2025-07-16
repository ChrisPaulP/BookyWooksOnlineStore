using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
using BookWooks.OrderApi.Infrastructure.Common.Behaviour;
using BookWooks.OrderApi.UseCases.Create;
using BookWooks.OrderApi.UseCases.Errors;
using BookWooks.OrderApi.UseCases.Orders;
using BookWooks.OrderApi.UseCases.Orders.Create;
using BookWooks.OrderApi.UseCases.Orders.Get;
using BookWooks.OrderApi.Web.Orders;
using BookyWooks.SharedKernel.Validation;
using LanguageExt;
using Logging;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NSubstitute;
using Xunit;
using static BookWooks.OrderApi.UseCases.Errors.DatabaseErrors;

namespace BookWooks.OrderApi.UnitTests.Endpoints;
public class CreateTests
{
  [Fact]
  public async Task HandleAsync_ReturnsOk_WhenOrderCreated()
  {
    // Arrange
    var request = CreateTestOrderRequest();

    // Replace with your actual Order DTO type and properties
    var orderId = OrderId.From(Guid.NewGuid());

    var mediator = new Mock<IMediator>();
    mediator
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Prelude.Right<CreateOrderErrors, OrderId>(orderId)));

    // Act
    var result = await Create.HandleAsync(request, mediator.Object, Substitute.For<IDiagnosticsActivityLogger>(), CancellationToken.None);

    // Assert
    var okResult = Assert.IsType<Ok<CreateOrderResponse>>(result);
    Assert.Equal(orderId.Value, okResult.Value!.Id);
    Assert.Equal(request.CustomerId, okResult.Value!.CustomerId);
  }

  [Fact]
    public async Task HandleAsync_ReturnsErrorValidation_WhenOrderCreationError()
    {
        var request = CreateTestOrderRequest();

        var errorDictionary = new Dictionary<ErrorType, IReadOnlyList<ValidationError>>
        {
            [ErrorType.DeliveryAddress] = [new ValidationError("Delivery address is incorrect")]
        };
        var errors = new ValidationErrors(errorDictionary);

        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Prelude.Left<CreateOrderErrors, OrderId>(new CreateOrderErrors(errors))));

        // Act
        var result = await Create.HandleAsync(request, mediator.Object, Substitute.For<IDiagnosticsActivityLogger>(), CancellationToken.None);
        var problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Contains("Validation", problemResult.ProblemDetails.Title, StringComparison.OrdinalIgnoreCase);

        // Assert
        Assert.False(result is Ok<CreateOrderResponse>);
    }
  [Fact]
  public async Task HandleAsync_ReturnsErrorResult_WhenDatabaseError()
  {
    // Arrange
    var request = CreateTestOrderRequest();
    var databaseError = new NetworkErrors(new DatabaseError()); // Wrap NetworkErrors in OrderErrors
    var mediator = new Mock<IMediator>();
    mediator.Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
      .Returns(Task.FromResult(Either< CreateOrderErrors, OrderId>.Left(databaseError)));


    // Act
    var result = await Create.HandleAsync(request, mediator.Object, Substitute.For<IDiagnosticsActivityLogger>(), CancellationToken.None);

    // Assert
    Assert.False(result is Ok<CreateOrderResponse>);
  }


  [Fact]
  public async Task HandleAsync_ReturnsErrorResult_WhenNetworkError_WithRealPipeline()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddMediatR(cfg => {
      cfg.RegisterServicesFromAssemblyContaining<CreateOrderCommandHandler>();
    });
    // Register your pipeline behavior
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));
    // Register a handler that throws TimeoutException
    services.AddTransient<IRequestHandler<CreateOrderCommand, Either<CreateOrderErrors, OrderId>>, ThrowingHandler>();

    var provider = services.BuildServiceProvider();
    var mediator = provider.GetRequiredService<IMediator>();

    var request = CreateTestOrderRequest();

    // Act
    var result = await Create.HandleAsync(request, mediator, Substitute.For<IDiagnosticsActivityLogger>(), CancellationToken.None);

    // Assert
    var problemResult = Assert.IsType<ProblemHttpResult>(result);
    Assert.Contains("Timeout", problemResult.ProblemDetails.Title, StringComparison.OrdinalIgnoreCase);
  }

  // Fix for CS0311: Ensure ThrowingHandler implements the correct IRequestHandler interface
  public class ThrowingHandler : IRequestHandler<CreateOrderCommand, Either<CreateOrderErrors, OrderId>>
  {
    public Task<Either<CreateOrderErrors, OrderId>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        => throw new TimeoutException("Simulated timeout");
  }
  private static CreateOrderRequest CreateTestOrderRequest()
  {
    var orderId = Guid.NewGuid();
    var customerId = Guid.NewGuid();
    var address = new OrderRequestAddress(
        Street: "123 Main St",
        City: "Testville",
        Country: "Testland",
        Postcode: "12345"
    );
    var payment = new OrderRequestPayment(
        CardHolderName: "John Doe",
        CardHolderNumber: "4111111111111111",
        ExpiryDate: "12/30",
        Cvv: "123",
        PaymentMethod: 1
    );
    var orderItems = new List<OrderRequestOrderItem>
    {
        new OrderRequestOrderItem(
            ProductId: Guid.NewGuid(),
            Price: 19.99m,
            Quantity: 2,
            ProductName: "Test Book",
            ProductDescription: "A dummy book for testing"
        )
    };

    return new CreateOrderRequest(orderId, customerId, address, payment, orderItems);
  }
}

