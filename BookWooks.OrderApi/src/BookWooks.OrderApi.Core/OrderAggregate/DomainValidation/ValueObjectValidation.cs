namespace BookWooks.OrderApi.Core.OrderAggregate.DomainValidation;

public static class ValueObjectValidation
{
  public static Validation ValidateGuid(Guid id, string fieldName)
  {
    var result = Validation.Invalid(string.Format(DomainValidationMessages.DoesNotMeetRequirements, fieldName));
    if (id == Guid.Empty)
      AddError(result, DomainValidationMessages.Required, fieldName);

    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }

  public static Validation ValidateString(string input,string fieldName,int minLength,int maxLength,Func<string, bool>? customRule = null,string? customRuleError = null,Func<string, bool>? customRule2 = null,string? customRuleError2 = null, bool mustBeExactLength = false)
  {
    var result = Validation.Invalid(string.Format(DomainValidationMessages.DoesNotMeetRequirements, fieldName));

    if (string.IsNullOrWhiteSpace(input))
      AddError(result, DomainValidationMessages.Required, fieldName);

    if (input.Length > maxLength)
      AddError(result, DomainValidationMessages.TooLong, fieldName, maxLength);

    if (input.Length < minLength)
      AddError(result, DomainValidationMessages.TooShort, fieldName, minLength);
    
    if (mustBeExactLength && input.Length != minLength)
      AddError(result, DomainValidationMessages.MustBeEqual, fieldName, minLength);

    if (customRule != null && !customRule(input) && !string.IsNullOrEmpty(customRuleError))
      AddError(result, customRuleError);

    if (customRule2 != null && !customRule2(input) && !string.IsNullOrEmpty(customRuleError2))
      AddError(result, customRuleError2);

    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }

  public static Validation ValidateDecimal(decimal input, string fieldName, int minValue)
  {
    var result = Validation.Invalid(string.Format(DomainValidationMessages.DoesNotMeetRequirements, fieldName));

    if (input <= minValue)
      AddError(result, DomainValidationMessages.PriceInvalid, fieldName, minValue);

    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }

  public static Validation ValidateInt(int input, string fieldName, int minValue)
  {
    var result = Validation.Invalid(string.Format(DomainValidationMessages.DoesNotMeetRequirements, fieldName));

    if (input <= minValue)
      AddError(result, DomainValidationMessages.QuantityInvalid, fieldName, minValue);

    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }
  public static Validation ValidateDateTime(DateTimeOffset dateTime, string fieldName)
  {
    var result = Validation.Invalid(string.Format(DomainValidationMessages.DoesNotMeetRequirements, fieldName));
    if (dateTime > DateTimeOffset.UtcNow)
     AddError(result, DomainValidationMessages.DateInvalid, fieldName);

    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }

  private static void AddError(Validation result, string validationMessage, params object[] args)
  {
    var message = string.Format(validationMessage, args);
    result.WithData(BusinessRuleError.Create(message), string.Empty);
  }
}


