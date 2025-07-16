using BookWooks.OrderApi.UseCases.Errors;
using static BookWooks.OrderApi.UseCases.Errors.DatabaseErrors;
using OrderErrors = BookWooks.OrderApi.UseCases.Errors.OrderErrors;

namespace BookWooks.OrderApi.Infrastructure.Common.Behaviour;

public class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
  private readonly ILogger<ErrorHandlingBehavior<TRequest, TResponse>> _logger;

  public ErrorHandlingBehavior(ILogger<ErrorHandlingBehavior<TRequest, TResponse>> logger) =>
      _logger = logger;

  public async Task<TResponse> Handle(
      TRequest request,
      RequestHandlerDelegate<TResponse> next,
      CancellationToken cancellationToken)
  {
    try
    {
      return await next();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unhandled exception in request {RequestName}", typeof(TRequest).Name);

      //return TryWrapExceptionIntoEither(ex) ?? throw new InvalidOperationException("Could not wrap exception");
      var wrapped = TryWrapExceptionIntoEither(ex);
      if (wrapped is not null)
        return wrapped;

      throw new InvalidOperationException($"Could not wrap exception of type {ex.GetType().Name}", ex);
    }
  }
  
  private TResponse? TryWrapExceptionIntoEither(Exception ex)
  {
    var type = typeof(TResponse);
    if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Either<,>))
      return default;

    var leftType = type.GetGenericArguments()[0];
    var rightType = type.GetGenericArguments()[1];

    if (!typeof(IError).IsAssignableFrom(leftType))
      return default;

    var error = ExceptionToErrorMapper.Map(ex, leftType);

    var leftMethod = typeof(Prelude)
        .GetMethods()
        .FirstOrDefault(m =>m.Name == "Left"&& m.IsGenericMethod&& m.GetParameters().Length == 1);

    if (leftMethod == null) return default;

    var genericLeft = leftMethod.MakeGenericMethod(leftType, rightType);
    var either = genericLeft.Invoke(null, [error]);

    return (TResponse)either!;
  }
  public static class ExceptionToErrorMapper
  {
    private static readonly Dictionary<(Type ExceptionType, Type ErrorType), Func<Exception, IError>> _mappings = new()
    {
        { (typeof(DbUpdateException), typeof(OrderErrors)), ex => new OrderErrors(new NetworkErrors(new DatabaseError())) },
        { (typeof(DbUpdateException), typeof(CreateOrderErrors)), ex => new CreateOrderErrors(new NetworkErrors(new DatabaseError())) },
        { (typeof(TimeoutException), typeof(OrderErrors)), ex => new OrderErrors(new NetworkErrors(new TimeoutError())) },
        { (typeof(TimeoutException), typeof(CreateOrderErrors)), ex => new CreateOrderErrors(new NetworkErrors(new TimeoutError())) }
    };

    public static IError Map(Exception ex, Type errorType)
    {
      if (_mappings.TryGetValue((ex.GetType(), errorType), out var factory))
        return factory(ex);

      if (errorType == typeof(CreateOrderErrors))
        return new CreateOrderErrors(new NetworkErrors(new UnexpectedError()));
      if (errorType == typeof(OrderErrors))
        return new OrderErrors(new NetworkErrors(new UnexpectedError()));
      // Optionally handle other error types or throw if not expected
      return new NetworkErrors(new UnexpectedError());
    }
  }
}

