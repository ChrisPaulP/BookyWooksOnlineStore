using BookWooks.OrderApi.Core.OrderAggregate.BusinessRuleErrors;
using BookyWooks.SharedKernel.Validation;

namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
public record struct Payment
{
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
public partial record struct CardName;
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct CardNumber;
[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct Expiration;

[ValueObject<int>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct PaymentMethod
{
  private static Vogen.Validation Validate(string input)
  {
    var result = Vogen.Validation.Invalid("Name does not meet requirements");

    var isNull = string.IsNullOrWhiteSpace(input);
    if (isNull) result.WithData(BusinessRuleError.Create("Name is required"), string.Empty);
    if (input is { Length: > 100 }) result.WithData(BusinessRuleError.Create("Name must be less than 50 characters"), string.Empty);
    if (input is { Length: < 3 }) result.WithData(BusinessRuleError.Create("Name must be greater than 3 characters"), string.Empty);

    var regex = ValidNameRegex();
    if (!isNull && !regex.IsMatch(input)) result.WithData(BusinessRuleError.Create("Name must contain only letters, hyphens, periods, and apostrophes"), string.Empty);

    return result.Data is { Count: > 0 }
        ? result
        : Vogen.Validation.Ok;
  }

  [GeneratedRegex(@"[a-zA-Z\-\.\'\s]*")]
  private static partial Regex ValidNameRegex();
}
