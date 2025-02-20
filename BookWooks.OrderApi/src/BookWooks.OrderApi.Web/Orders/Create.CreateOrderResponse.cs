using BookWooks.OrderApi.UseCases.Errors;
using BookyWooks.SharedKernel.ResultPattern;
using BookyWooks.SharedKernel.Validation;

namespace BookWooks.OrderApi.Web.Orders;

public record CreateOrderResponse(Guid Id, Guid CustomerId) : IResponse;
public record BusinessRuleViolationsResponse(string Message,int StatusCode, ValidationErrors Errors) : IResponse;
public record BusinessRuleViolationResponse(string Message, int StatusCode, Error Error) : IResponse;
public record ValidationProblemDetails(int StatusCode, string title, string detail, string instance, IDictionary<string, string[]> errors) : IResponse;


