namespace BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
public record DeliveryAddress
{
  public string Street { get; private set; }

  public string City { get; private set; }

  public string Country { get; private set; }

  public string PostCode { get; private set; }



  private DeliveryAddress(string street, string city, string country, string postCode)
  {
    Street = street ?? throw new ArgumentNullException(nameof(street));
    City = city ?? throw new ArgumentNullException(nameof(city));
    Country = country ?? throw new ArgumentNullException(nameof(country));
    PostCode = postCode ?? throw new ArgumentNullException(nameof(postCode));
  }
  public static DeliveryAddress Of(string street, string city, string country, string postCode)
  {
    ArgumentException.ThrowIfNullOrEmpty(street);
    ArgumentException.ThrowIfNullOrEmpty(city);
    ArgumentException.ThrowIfNullOrEmpty(country);
    ArgumentException.ThrowIfNullOrEmpty(postCode);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(postCode.Length, 10);

    return new DeliveryAddress(street, city, country, postCode);
  }
}
