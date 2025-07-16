using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
using BookWooks.OrderApi.Infrastructure.Common.Behaviour;
using BookWooks.OrderApi.UseCases.Create;
using BookWooks.OrderApi.UseCases.Errors;
using BookWooks.OrderApi.UseCases.Orders;
using BookWooks.OrderApi.UseCases.Orders.Get;
using BookWooks.OrderApi.Web.Orders;
using BookWooks.OrderApi.Web.Shared;
using BookyWooks.SharedKernel.Validation;
using LanguageExt;
using Logging;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NSubstitute;
using Xunit;
using static BookWooks.OrderApi.UseCases.Errors.DatabaseErrors;


public class GetOrderDetailsTests
{
    [Fact]
    public async Task HandleAsync_ReturnsOk_WhenOrderFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new GetOrderDetailsRequest { OrderId = orderId };

        // Replace with your actual Order DTO type and properties
        var order = new OrderDTOs(orderId,"Completed");

        var mediator = new Mock<IMediator>();
        mediator
                .Setup(m => m.Send(It.IsAny<GetOrderDetailsQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Prelude.Right<OrderErrors, OrderDTOs>(order)));

        // Act
        var result = await GetOrderDetails.HandleAsync(request, mediator.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<GetOrderDetailsResponse>>(result);
        Assert.Equal(orderId, okResult.Value!.id);
        Assert.Equal("Completed", okResult.Value!.status);
    }

    [Fact]
    public async Task HandleAsync_ReturnsErrorResult_WhenOrderError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new GetOrderDetailsRequest { OrderId = orderId };
        var error = new OrderNotFound();
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetOrderDetailsQuery>(), It.IsAny<CancellationToken>()))
          .Returns(Task.FromResult(Prelude.Left<OrderErrors, OrderDTOs>(error)));


        // Act
        var result = await GetOrderDetails.HandleAsync(request, mediator.Object, CancellationToken.None);

        // Assert
        Assert.False(result is Ok<GetOrderDetailsResponse>);
    }

  [Fact]
  public async Task HandleAsync_ReturnsErrorResult_WhenDatabaseError()
  {
    // Arrange
    var orderId = Guid.NewGuid();
    var request = new GetOrderDetailsRequest { OrderId = orderId };
    var databaseError = new NetworkErrors(new DatabaseError()); // Wrap NetworkErrors in OrderErrors
    var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetOrderDetailsQuery>(), It.IsAny<CancellationToken>()))
          .Returns(Task.FromResult(Either<OrderErrors, OrderDTOs>.Left(databaseError)));


    // Act
    var result = await GetOrderDetails.HandleAsync(request, mediator.Object, CancellationToken.None);

    // Assert
    Assert.False(result is Ok<GetOrderDetailsResponse>);
  }


  [Fact]
  public async Task HandleAsync_ReturnsErrorResult_WhenNetworkError_WithRealPipeline()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddMediatR(cfg => {
      cfg.RegisterServicesFromAssemblyContaining<GetOrderDetailsHandler>();
    });
    // Register your pipeline behavior
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));
    // Register a handler that throws TimeoutException
    services.AddTransient<IRequestHandler<GetOrderDetailsQuery, Either<OrderErrors, OrderDTOs>>, ThrowingHandler>();

    var provider = services.BuildServiceProvider();
    var mediator = provider.GetRequiredService<IMediator>();

    var orderId = Guid.NewGuid();
    var request = new GetOrderDetailsRequest { OrderId = orderId };

    // Act
    var result = await GetOrderDetails.HandleAsync(request, mediator, CancellationToken.None);

    // Assert
    var problemResult = Assert.IsType<ProblemHttpResult>(result);
    Assert.Contains("Timeout", problemResult.ProblemDetails.Title, StringComparison.OrdinalIgnoreCase);
  }

    // Fix for CS0311: Ensure ThrowingHandler implements the correct IRequestHandler interface
    public class ThrowingHandler : IRequestHandler<GetOrderDetailsQuery, Either<OrderErrors, OrderDTOs>>
    {
        public Task<Either<OrderErrors, OrderDTOs>> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
            => throw new TimeoutException("Simulated timeout");
    }
}

// Dummy OrderDTO for test compilation; replace with your actual DTO if needed

