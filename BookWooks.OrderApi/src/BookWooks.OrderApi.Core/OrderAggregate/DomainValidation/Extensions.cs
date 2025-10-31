namespace BookWooks.OrderApi.Core.OrderAggregate.DomainValidation;
public static class Extensions
{
  public static Validation<TValidationError, T> ToValidationMonad<TValidationError, T>(
      this ValueObjectOrError<T> validation,
      Func<IReadOnlyList<BusinessRuleError>, TValidationError> onError)
      => validation.IsSuccess switch
      {
        true => validation.ValueObject,
        false => onError(validation.Error.Data?.Keys.OfType<BusinessRuleError>().ToList() ?? [])
      };
}
