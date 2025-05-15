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
    var maybeName =        ProductName.TryFrom(name).ToValidationMonad(errors => new ProductValidationErrors("ProductName", errors));
    var maybeDescription = ProductDescription.TryFrom(description).ToValidationMonad(errors => new ProductValidationErrors("Product Description Error", errors));
    var maybePrice =       ProductPrice.TryFrom(price).ToValidationMonad(errors => new ProductValidationErrors("Product Price Error", errors));
    var maybeQuantity =    ProductQuantity.TryFrom(quantity).ToValidationMonad(errors => new ProductValidationErrors("Product Quantity Error", errors));

    return (maybeName, maybeDescription, maybePrice, maybeQuantity).Apply((nameVo, descriptionVo, priceVo, quantityVo) =>
    {
      var product = new Product(ProductId.New(), nameVo, descriptionVo, priceVo, quantityVo);
      return product;
    });
  }
}
