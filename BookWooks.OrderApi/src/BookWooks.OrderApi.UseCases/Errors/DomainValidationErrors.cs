using OneOf;
using static BookWooks.OrderApi.UseCases.Errors.DatabaseErrors;

namespace BookWooks.OrderApi.UseCases.Errors;

public record DomainValidationErrors(IReadOnlyDictionary<ErrorType, IReadOnlyList<DomainValidationError>> Errors) : IError;
public enum ErrorType
{
  Customer,
  DeliveryAddress,
  Payment,
  OrderItem,
  OrderId,
  Order,
  OrderStatusValidationError
}



