
using BookyWooks.SharedKernel.Validation;


namespace BookWooks.OrderApi.Web;

public static class Extensions
{
  public static IResult ToProblem(this Error errors)
  {
    if (errors == null)
    {
      return Results.Problem();
    }

    return CreateProblem(errors);
  }

  private static IResult CreateProblem(Error errors)
  {
    var statusCode = errors.HttpErrorType switch
    {
      HttpErrorType.Conflict => StatusCodes.Status409Conflict,
      HttpErrorType.Validation => StatusCodes.Status400BadRequest,
      HttpErrorType.NotFound => StatusCodes.Status404NotFound,
      HttpErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
      _ => StatusCodes.Status500InternalServerError
    };

    return Results.Problem(type: errors.TypeName,
        title: errors.Code,
        detail: errors.Message,
        statusCode: statusCode);
  }
}
