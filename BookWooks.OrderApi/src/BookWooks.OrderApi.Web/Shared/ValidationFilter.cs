namespace BookWooks.OrderApi.Web.Shared;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
  private readonly IValidator<T> _validator;

  public ValidationFilter(IValidator<T> validator)
  {
    _validator = validator;
  }

  public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
  {
    if (context.Arguments[0] is not T request) 
    {
      return Results.BadRequest("Invalid request payload");
    }

    var validationResult = await _validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
      return new ValidationProblemDetails(StatusCodes.Status400BadRequest, "Validation failed", "One or more validation errors occured", CreateOrderRequest.Route, validationResult.ToDictionary());
    }

    return await next(context);
  }
}

