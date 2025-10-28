namespace BookWooks.OrderApi.Core.OrderAggregate.Entities;
public record Product : EntityBase, IAggregateRoot
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

  public static Validation<ProductValidationErrors, Product> CreateProduct(Guid productId, string name, string description, decimal price, int quantity)
  {
    var productIdValidation =   ProductId.TryFrom(productId).ToValidationMonad(errors => new ProductValidationErrors(DomainValidationMessages.ProductId, errors));
    var nameValidation =        ProductName.TryFrom(name).ToValidationMonad(errors => new ProductValidationErrors(DomainValidationMessages.CardName, errors));
    var descriptionValidation = ProductDescription.TryFrom(description).ToValidationMonad(errors => new ProductValidationErrors(DomainValidationMessages.ProductDescription, errors));
    var priceValidation =       ProductPrice.TryFrom(price).ToValidationMonad(errors => new ProductValidationErrors(DomainValidationMessages.ProductPrice, errors));
    var quantityValidation =    ProductQuantity.TryFrom(quantity).ToValidationMonad(errors => new ProductValidationErrors(DomainValidationMessages.ProductQuantity, errors));

    return (productIdValidation, nameValidation, descriptionValidation, priceValidation, quantityValidation).Apply((createdProductId, createdName, createdDescription, createdPrice, createdQuantity) =>
    {
      var product = new Product(createdProductId, createdName, createdDescription, createdPrice, createdQuantity);
      return product;
    });
  }
}
