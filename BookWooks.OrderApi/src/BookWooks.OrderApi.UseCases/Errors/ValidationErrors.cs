namespace BookWooks.OrderApi.UseCases.Errors;

public record ValidationErrors(IReadOnlyDictionary<ErrorType, IReadOnlyList<ValidationError>> Errors);
public enum ErrorType
{
  Customer,
  DeliveryAddress,
  Payment,
  OrderItem
}



