namespace BookWooks.OrderApi.Core.OrderAggregate.DomainValidation;

public static class ValueObjectValidation
{
  public static Validation Validate(Guid id, string fieldName)
  {
    var result = Validation.Invalid(string.Format(ValidationMessages.DoesNotMeetRequirements, fieldName));
    if (id == Guid.Empty)
      AddError(result, ValidationMessages.Required, fieldName);

    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }

  public static Validation Validate(string input,string fieldName,int minLength,int maxLength,Func<string, bool>? customRule = null,string? customRuleError = null,Func<string, bool>? customRule2 = null,string? customRuleError2 = null, bool mustBeExactLength = false)
  {
    var result = Validation.Invalid(string.Format(ValidationMessages.DoesNotMeetRequirements, fieldName));

    if (string.IsNullOrWhiteSpace(input))
      AddError(result, ValidationMessages.Required, fieldName);

    if (input.Length > maxLength)
      AddError(result, ValidationMessages.TooLong, fieldName, maxLength);

    if (input.Length < minLength)
      AddError(result, ValidationMessages.TooShort, fieldName, minLength);
    
    if (mustBeExactLength && input.Length != minLength)
      AddError(result, ValidationMessages.MustBeEqual, fieldName, minLength);

    if (customRule != null && !customRule(input) && !string.IsNullOrEmpty(customRuleError))
      AddError(result, customRuleError);

    if (customRule2 != null && !customRule2(input) && !string.IsNullOrEmpty(customRuleError2))
      AddError(result, customRuleError2);

    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }

  public static Validation Validate(decimal input, string fieldName, int minValue)
  {
    var result = Validation.Invalid(string.Format(ValidationMessages.DoesNotMeetRequirements, fieldName));

    if (input <= minValue)
      AddError(result, ValidationMessages.PriceInvalid, fieldName, minValue);

    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }

  public static Validation Validate(int input, string fieldName, int minValue)
  {
    var result = Validation.Invalid(string.Format(ValidationMessages.DoesNotMeetRequirements, fieldName));

    if (input <= minValue)
      AddError(result, ValidationMessages.QuantityInvalid, fieldName, minValue);

    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }
  public static Validation Validate(DateTimeOffset dateTime, string fieldName)
  {
    var result = Validation.Invalid(string.Format(ValidationMessages.DoesNotMeetRequirements, fieldName));
    if (dateTime > DateTimeOffset.UtcNow)
     AddError(result, ValidationMessages.DateInvalid, fieldName);

    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }

  private static void AddError(Validation result, string validationMessage, params object[] args)
  {
    var message = string.Format(validationMessage, args);
    result.WithData(BusinessRuleError.Create(message), string.Empty);
  }
}


