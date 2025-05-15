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
    var maybeCardName =
        CardName.TryFrom(cardName)
            .ToValidationMonad<OrderValidationErrors, CardName>(error => new PaymentErrors(cardName, error));
    var maybeCardNumber =
        CardNumber.TryFrom(cardNumber)
            .ToValidationMonad<OrderValidationErrors, CardNumber>(error => new PaymentErrors(cardName, error));
    var maybeExpiration =
        Expiration.TryFrom(expiration)
            .ToValidationMonad<OrderValidationErrors, Expiration>(error => new PaymentErrors(cardName, error));
    var maybePaymentMethod =
        PaymentMethod.TryFrom(paymentMethod)
            .ToValidationMonad<OrderValidationErrors, PaymentMethod>(error => new PaymentErrors(cardName, error));

    return (maybeCardName, maybeCardNumber, maybeExpiration, maybePaymentMethod)
        .Apply((cardNameVo, cardNumberVo, expirationVo, paymentMethodVo) => new Payment(cardNameVo, cardNumberVo, expirationVo, paymentMethodVo));
  }

}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct CardName
{
  private static Vogen.Validation Validate(string input)
  {
    var result = Vogen.Validation.Invalid("Card Name does not meet requirements");

    if (string.IsNullOrWhiteSpace(input))
      result.WithData(BusinessRuleError.Create("Card Name is required"), string.Empty);
    if (input.Length > 50)
      result.WithData(BusinessRuleError.Create("Card Name must be less than 50 characters"), string.Empty);
    if (input.Length < 3)
      result.WithData(BusinessRuleError.Create("Card Name must be greater than 3 characters"), string.Empty);

    var regex = ValidNameRegex();
    if (!string.IsNullOrWhiteSpace(input) && !regex.IsMatch(input))
      result.WithData(BusinessRuleError.Create("Card Name must contain only letters, hyphens, periods, and apostrophes"), string.Empty);

    return result.Data is { Count: > 0 } ? result : Vogen.Validation.Ok;
  }

  private static Regex ValidNameRegex() => new Regex(@"[a-zA-Z\-\.\'\s]*");
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct CardNumber
{
  private static Vogen.Validation Validate(string input)
  {
    var result = Vogen.Validation.Invalid("Card Number does not meet requirements");

    if (string.IsNullOrWhiteSpace(input))
      result.WithData(BusinessRuleError.Create("Card Number is required"), string.Empty);
    if (input.Length != 16)
      result.WithData(BusinessRuleError.Create("Card Number must be 16 digits"), string.Empty);

    var regex = ValidCardNumberRegex();
    if (!string.IsNullOrWhiteSpace(input) && !regex.IsMatch(input))
      result.WithData(BusinessRuleError.Create("Card Number must contain only digits"), string.Empty);

    return result.Data is { Count: > 0 } ? result : Vogen.Validation.Ok;
  }

  private static Regex ValidCardNumberRegex() => new Regex(@"^\d{16}$");
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct Expiration
{
  private static Vogen.Validation Validate(string input)
  {
    var result = Vogen.Validation.Invalid("Expiration date does not meet requirements");

    if (string.IsNullOrWhiteSpace(input))
      result.WithData(BusinessRuleError.Create("Expiration date is required"), string.Empty);

    var regex = ValidExpirationRegex();
    if (!string.IsNullOrWhiteSpace(input) && !regex.IsMatch(input))
      result.WithData(BusinessRuleError.Create("Expiration date must be in MM/YY format"), string.Empty);

    return result.Data is { Count: > 0 } ? result : Vogen.Validation.Ok;
  }

  private static Regex ValidExpirationRegex() => new Regex(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$");
}

[ValueObject<int>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct PaymentMethod
{
  private static Vogen.Validation Validate(int input)
  {
    var result = Vogen.Validation.Invalid("Payment Method does not meet requirements");

    if (input < 0 || input > 10) // Assuming valid payment methods are between 0 and 10
    {
      result.WithData(BusinessRuleError.Create("Invalid payment method"), string.Empty);
    }

    return result.Data is { Count: > 0 } ? result : Vogen.Validation.Ok;
  }
}
