namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
public record Payment
{
  private Payment() { }
  private Payment(CardName cardName, CardNumber cardNumber, Expiration expiraton, PaymentMethod paymentMethod)
  {
    CardName = cardName;
    CardNumber = cardNumber;
    Expiration = expiraton;
    PaymentMethod = paymentMethod;
  }

  public CardName CardName { get; }
  public CardNumber CardNumber { get; }
  public Expiration Expiration { get; }
  public PaymentMethod PaymentMethod { get; }

  public static Validation<OrderValidationErrors, Payment> TryCreate(
       string cardName, string cardNumber, string expiration, int paymentMethod)
  {
    var cardNameValidation =CardName.TryFrom(cardName).ToValidationMonad<OrderValidationErrors, CardName>(error => new PaymentErrors(cardName, error));
    var cardNumberValidation =CardNumber.TryFrom(cardNumber).ToValidationMonad<OrderValidationErrors, CardNumber>(error => new PaymentErrors(cardName, error));
    var expirationValidation = Expiration.TryFrom(expiration).ToValidationMonad<OrderValidationErrors, Expiration>(error => new PaymentErrors(cardName, error));
    var paymentMethodValidation =PaymentMethod.TryFrom(paymentMethod).ToValidationMonad<OrderValidationErrors, PaymentMethod>(error => new PaymentErrors(cardName, error));

    return (cardNameValidation, cardNumberValidation, expirationValidation, paymentMethodValidation)
        .Apply((createdCardName, createdCardNumber, createdExpiration, createdPaymentMethod) => new Payment(createdCardName, createdCardNumber, createdExpiration, createdPaymentMethod));
  }
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct CardName
{
  private static Validation Validate(string input) => ValueObjectValidation.ValidateString(input, ValidationMessages.CardName, minLength: 3, maxLength: 50, customRule: input => ValidNameRegex().IsMatch(input), customRuleError: ValidationMessages.CardNameInvalid);
  private static Regex ValidNameRegex() => new Regex(@"[a-zA-Z\-\.\'\s]*");
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct CardNumber
{
  private static Validation Validate(string input) => ValueObjectValidation.ValidateString(input, ValidationMessages.CardNumber, minLength: 16, maxLength: 16, customRule: input => ValidCardNumberRegex().IsMatch(input), customRuleError: ValidationMessages.CardNumberInvalid, mustBeExactLength: true);
  private static Regex ValidCardNumberRegex() => new Regex(@"^\d{16}$");
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct Expiration
{
  private static Vogen.Validation Validate(string input) => ValueObjectValidation.ValidateString(input, ValidationMessages.Expiration, minLength: 0, maxLength: 25, customRule: input => ValidExpirationRegex().IsMatch(input), customRuleError: ValidationMessages.ExpirationInvalid);
  private static Regex ValidExpirationRegex() => new Regex(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$");
}

[ValueObject<int>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct PaymentMethod
{
  private static Vogen.Validation Validate(int input) => ValueObjectValidation.ValidateInt(input, ValidationMessages.Expiration, minValue: 0);
}
