using BookyWooks.SharedKernel.Validation;
using static BookWooks.OrderApi.UseCases.Errors.DatabaseErrors;

namespace BookWooks.OrderApi.Web.Shared;

public static class ToHttpResult
{
  public record ErrorDefinition(string Title, int Status, string TypeUrl);

  public static class Errors
  {
    public static readonly ErrorDefinition ValidationError = new("Validation error", 400, "https://httpstatuses.com/400");
    public static readonly ErrorDefinition OrderNotFound = new("Order not found", 404, "https://httpstatuses.com/404");
    public static readonly ErrorDefinition DatabaseError = new("Database error", 500, "https://httpstatuses.com/500");
    public static readonly ErrorDefinition TimeoutError = new("Timeout error", 500, "https://httpstatuses.com/500");
    public static readonly ErrorDefinition UnexpectedError = new("Unexpected error", 500, "https://httpstatuses.com/500");
    public static readonly ErrorDefinition UnhandledError = new("Unhandled error", 500, "https://httpstatuses.com/500");
  }

  public static IResult Map(OrderErrors error) => Map(UnwrapError(error));
  public static IResult Map(CreateOrderErrors error) => Map(UnwrapError(error));

  public static IResult Map(IError error)
  {
    var concreteError = UnwrapError(error);
    var returnedError = GetErrorDefinition(concreteError);

      var pd = new ProblemDetails
      {
        Title = returnedError.Title,
        Detail = (concreteError as Error)?.Message ?? "Error",
        Status = returnedError.Status,
        Type = returnedError.TypeUrl
      };
    foreach (var kv in CreateExtensions(concreteError))
        pd.Extensions[kv.Key] = kv.Value;

    // If it's a ValidationErrors, add the errors dictionary to extensions
    if (concreteError is DomainValidationErrors ve)
      pd.Extensions["validationErrors"] = ve.Errors;

    return Results.Problem(pd);
  }

  private static ErrorDefinition GetErrorDefinition(IError error) => error switch
  {
    DomainValidationErrors => Errors.ValidationError,
    OrderNotFound => Errors.OrderNotFound,
    DatabaseError => Errors.DatabaseError,
    TimeoutError => Errors.TimeoutError,
    UnexpectedError => Errors.UnexpectedError,
    UnhandledError => Errors.UnhandledError,
    _ => new ErrorDefinition("Unknown error", 500, "")
  };

  private static IError UnwrapError(IError error)
  {
    while (true)
    {
      if (error is OrderErrors { Value: IError orderError }) { error = orderError; continue; }
      if (error is NetworkErrors { Value: IError networkError }) { error = networkError; continue; }
      if (error is CreateOrderErrors { Value: IError createOrderError }) { error = createOrderError; continue; }
      return error;
    }
  }
  private static IDictionary<string, object?> CreateExtensions(IError error) =>
      new Dictionary<string, object?> { ["errorType"] = error.GetType().Name };
}
