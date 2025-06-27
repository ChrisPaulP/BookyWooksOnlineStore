namespace BookWooks.OrderApi.Core.OrderAggregate.DomainValidation;
public static class Extensions
{
  public static Validation<TValidationError, T> ToValidationMonad<TValidationError, T>(
      this ValueObjectOrError<T> validation,
      Func<IReadOnlyList<BusinessRuleError>, TValidationError> onError)
      => validation.IsSuccess switch
      {
        true => validation.ValueObject,
        false => onError(validation.Error.Data?.Values.OfType<BusinessRuleError>().ToList() ?? [])
      };

  public static Either<TError, T> ToEither<TError, T>(
        this ValueObjectOrError<T> validation,
        Func<IReadOnlyList<string>, TError> onError)
  {
    return validation.IsSuccess
        ? Prelude.Right<TError, T>(validation.ValueObject)
        : Prelude.Left<TError, T>(onError(validation.Error.Data?.Values.OfType<string>().ToList() ?? []));
  }
}
