using OneOf;
using static BookWooks.OrderApi.UseCases.Errors.DatabaseErrors;

namespace BookWooks.OrderApi.UseCases.Errors;

public record ValidationErrors(IReadOnlyDictionary<ErrorType, IReadOnlyList<ValidationError>> Errors) : IError;
public enum ErrorType
{
  Customer,
  DeliveryAddress,
  Payment,
  OrderItem,
  OrderId,
  Order
}



