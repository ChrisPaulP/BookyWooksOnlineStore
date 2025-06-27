namespace BookWooks.OrderApi.Core.OrderAggregate.Entities;

public record OrderItem(OrderItemId OrderItemId, OrderId OrderId,ProductId ProductId,ProductPrice Price,ProductQuantity Quantity,ProductName ProductName,ProductDescription ProductDescription) : EntityBase
{
  public Validation<OrderItemValidationErrors, OrderItem> UpdateQuantity(int newQuantity)
  {
    var quantityValidation = ProductQuantity.TryFrom(newQuantity).ToValidationMonad(error => new OrderItemValidationErrors(ValidationMessages.ProductQuantity, error));

    return quantityValidation.Apply(createdQuantity => this with { Quantity = (ProductQuantity)createdQuantity });
  }

  public Validation<OrderItemValidationErrors, OrderItem> UpdatePrice(int newPrice)
  {
    var updatedPriceValidation = ProductPrice.TryFrom(newPrice).ToValidationMonad(error => new OrderItemValidationErrors(ValidationMessages.ProductPrice, error));
    return updatedPriceValidation.Apply(createdPrice => this with { Price = (ProductPrice)createdPrice });
  }

  internal static Validation<OrderItemValidationErrors, OrderItem> AddOrderItem(
      Guid orderId, Guid productId, decimal price, int quantity, string productName, string productDescription)
  {
    var orderIdValidation = OrderId.TryFrom(orderId).ToValidationMonad(errors => new OrderItemValidationErrors(ValidationMessages.OrderId, errors));
    var productIdValidation = ProductId.TryFrom(productId).ToValidationMonad(errors => new OrderItemValidationErrors(ValidationMessages.ProductId, errors));
    var priceValidation = ProductPrice.TryFrom(price).ToValidationMonad(errors => new OrderItemValidationErrors(ValidationMessages.ProductPrice, errors));
    var quantityValidation = ProductQuantity.TryFrom(quantity).ToValidationMonad(errors => new OrderItemValidationErrors(ValidationMessages.ProductQuantity, errors));
    var productNameValidation = ProductName.TryFrom(productName).ToValidationMonad(errors => new OrderItemValidationErrors(ValidationMessages.ProductName, errors));
    var productDescriptionValidation = ProductDescription.TryFrom(productDescription).ToValidationMonad(errors => new OrderItemValidationErrors(ValidationMessages.ProductDescription, errors));

    return (orderIdValidation, productIdValidation, priceValidation, quantityValidation, productNameValidation, productDescriptionValidation)
        .Apply((createdOrderId, createdProductId, createdPrice, createdQuantity, createdProductName, createdProductDescription) =>
            new OrderItem(OrderItemId.New(), createdOrderId, createdProductId, createdPrice, createdQuantity, createdProductName, createdProductDescription)
        );
  }
}
