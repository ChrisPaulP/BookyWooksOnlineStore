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
    var streetValidation = Street.TryFrom(street).ToValidationMonad<OrderValidationErrors, Street>(error => new DeliveryAddressErrors(street, error));
    var cityValidation = City.TryFrom(city).ToValidationMonad<OrderValidationErrors, City>(error => new DeliveryAddressErrors(city, error));
    var countryValidation = Country.TryFrom(country).ToValidationMonad<OrderValidationErrors, Country>(error => new DeliveryAddressErrors(country, error));
    var postCodeValidation = PostCode.TryFrom(postcode).ToValidationMonad<OrderValidationErrors, PostCode>(error => new DeliveryAddressErrors(postcode, error));

    return (streetValidation, cityValidation, countryValidation, postCodeValidation)
        .Apply((createdStreet, createdCity, createdCountry, createdPostCode) => new DeliveryAddress(createdStreet, createdCity, createdCountry, createdPostCode));
  }
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct Street
{
  private static Validation Validate(string input) =>ValueObjectValidation.ValidateString(input,ValidationMessages.Street,minLength: 3,maxLength: 100,customRule: s => ValidStreetRegex().IsMatch(s), customRuleError: ValidationMessages.StreetInvalid);

  [GeneratedRegex(@"[a-zA-Z\-\.\'\s]*")]
  private static partial Regex ValidStreetRegex();
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct City
{
  private static Validation Validate(string input) => ValueObjectValidation.ValidateString(input,ValidationMessages.City,minLength: 1, maxLength: 50
      );
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct Country
{
  private static Validation Validate(string input) => ValueObjectValidation.ValidateString(input,ValidationMessages.Country,minLength: 1,maxLength: 50
      );
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct PostCode
{
  private static Validation Validate(string input) =>ValueObjectValidation.ValidateString(input,ValidationMessages.PostCode, minLength: 1, maxLength: 10,customRule: s => ValidPostCodeRegex().IsMatch(s),customRuleError: ValidationMessages.PostCodeInvalid);

  [GeneratedRegex(@"[a-zA-Z\-\.\'\s]*")]
  private static partial Regex ValidPostCodeRegex();
}





