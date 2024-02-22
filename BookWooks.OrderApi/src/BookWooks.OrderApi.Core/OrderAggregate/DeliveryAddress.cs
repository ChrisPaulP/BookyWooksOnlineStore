namespace BookWooks.OrderApi.Core.OrderAggregate;
public class DeliveryAddress : ValueObject
{
  public string Street { get; private set; }

  public string City { get; private set; }

  public string Country { get; private set; }

  public string PostCode { get; private set; }



  public DeliveryAddress(string street, string city, string country, string postCode)
  {
    Street = street ?? throw new ArgumentNullException(nameof(street));
    City = city ?? throw new ArgumentNullException(nameof(city));
    Country = country ?? throw new ArgumentNullException(nameof(country));
    PostCode = postCode ?? throw new ArgumentNullException(nameof(postCode));
  }

  public static DeliveryAddress InvalidPostCode(string postCode)
  {
    if (postCode.Length > 10)
    {
      //throw new InvalidPostCodeDomainException(postCode);
    }
    return new DeliveryAddress("", "", "", postCode);
  }
  protected override IEnumerable<object> GetEqualityComponents()
  {
    // Using a yield return statement to return each element one at a time
    yield return Street;
    yield return City;
    yield return Country;
    yield return PostCode;
  }
}
