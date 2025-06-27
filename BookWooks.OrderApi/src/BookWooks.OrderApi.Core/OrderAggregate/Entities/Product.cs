namespace BookWooks.OrderApi.Core.OrderAggregate.Entities;
public record Product : EntityBase
{
  public ProductId ProductId { get; set; }
  public ProductName Name { get; private set; }
  public ProductDescription Description { get; private set; }
  public ProductPrice Price { get; private set; }
  public ProductQuantity Quantity { get; private set; }
  private Product()
  {
    ProductId = ProductId.New();
  }
  private Product(ProductId id, ProductName name, ProductDescription description, ProductPrice price, ProductQuantity quantity)
  {
    ProductId = id;
    Description = description;
    Name = name;
    Price = price;
    Quantity = quantity;
  }

  public static Validation<ProductValidationErrors, Product> CreateProduct(string name, string description, decimal price, int quantity)
  {
    var productId =             ProductId.New();
    var nameValidation =        ProductName.TryFrom(name).ToValidationMonad(errors => new ProductValidationErrors(ValidationMessages.CardName, errors));
    var descriptionValidation = ProductDescription.TryFrom(description).ToValidationMonad(errors => new ProductValidationErrors(ValidationMessages.ProductDescription, errors));
    var priceValidation =       ProductPrice.TryFrom(price).ToValidationMonad(errors => new ProductValidationErrors(ValidationMessages.ProductPrice, errors));
    var quantityValidation =    ProductQuantity.TryFrom(quantity).ToValidationMonad(errors => new ProductValidationErrors(ValidationMessages.ProductQuantity, errors));

    return (nameValidation, descriptionValidation, priceValidation, quantityValidation).Apply((createdName, createdDescription, createdPrice, createdQuantity) =>
    {
      var product = new Product(productId, createdName, createdDescription, createdPrice, createdQuantity);
      return product;
    });
  }
}
