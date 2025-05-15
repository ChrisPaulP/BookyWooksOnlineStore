namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;

public record DeliveryAddress
{
  private DeliveryAddress(Street street, City city, Country country, PostCode postcode)
  {
    Street = street;
    City = city;
    Country = country;
    Postcode = postcode;
  }

  public Street Street { get; }
  public City City { get; }
  public Country Country { get; }
  public PostCode Postcode { get; }

  public static Validation<OrderValidationErrors, DeliveryAddress> TryCreate(
       string street, string city, string country, string postcode)
  {
    var maybeStreet =
        Street.TryFrom(street)
            .ToValidationMonad<OrderValidationErrors, Street>(error => new DeliveryAddressErrors(street, error));
    var maybeCity =
        City.TryFrom(city)
            .ToValidationMonad<OrderValidationErrors, City>(error => new DeliveryAddressErrors(city, error));
    var maybeCountry =
        Country.TryFrom(country)
            .ToValidationMonad<OrderValidationErrors, Country>(error => new DeliveryAddressErrors(country, error));
    var maybePostCode =
        PostCode.TryFrom(postcode)
            .ToValidationMonad<OrderValidationErrors, PostCode>(error => new DeliveryAddressErrors(postcode, error));

    return (maybeStreet, maybeCity, maybeCountry, maybePostCode)
        .Apply((streetVo, cityVo, countryVo, postCodeVo) => new DeliveryAddress(streetVo, cityVo, countryVo, postCodeVo));
  }
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct Street
{
  private static Validation Validate(string input)
  {
    var result = Validation.Invalid("Street does not meet requirements");

    if (string.IsNullOrWhiteSpace(input)) result.WithData(BusinessRuleError.Create("Street is required"), string.Empty);

    if (input.Length > 100) result.WithData(BusinessRuleError.Create("Street must be less than 100 characters"), string.Empty);

    if (input.Length < 3) result.WithData(BusinessRuleError.Create("Street must be greater than 3 characters"), string.Empty);

    var regex = ValidStreetRegex();
    if (!regex.IsMatch(input)) result.WithData(BusinessRuleError.Create("Street must contain only letters, numbers, spaces, hyphens, and apostrophes"), string.Empty);

    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }

  [GeneratedRegex(@"[a-zA-Z\-\.\'\s]*")]
  private static partial Regex ValidStreetRegex();
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct City
{
  private static Validation Validate(string input)
  {
    var result = Validation.Invalid("City does not meet requirements");
    if (string.IsNullOrWhiteSpace(input)) result.WithData(BusinessRuleError.Create("City is required"), string.Empty);

    if (input.Length > 50) result.WithData(BusinessRuleError.Create("City must be less than 50 characters"), string.Empty);

    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct Country
{
  private static Validation Validate(string input)
  {
    var result = Validation.Invalid("Country does not meet requirements");
    if (string.IsNullOrWhiteSpace(input)) result.WithData(BusinessRuleError.Create("Country is required"), string.Empty);

    if (input.Length > 50) result.WithData(BusinessRuleError.Create("Country must be less than 50 characters"), string.Empty);

    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct PostCode
{
  private static Validation Validate(string input)
  {
    var result = Validation.Invalid("Post Code does not meet requirements");
    if (string.IsNullOrWhiteSpace(input)) result.WithData(BusinessRuleError.Create("PostCode is required"), string.Empty);

    if (input.Length > 10) result.WithData(BusinessRuleError.Create("PostCode must be less than 10 characters"), string.Empty);

    var regex = ValidPostCodeRegex();
    if (!regex.IsMatch(input)) result.WithData(BusinessRuleError.Create("PostCode must be alphanumeric and can include spaces"), string.Empty);

    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }

  [GeneratedRegex(@"[a-zA-Z\-\.\'\s]*")]
  private static partial Regex ValidPostCodeRegex();
}
