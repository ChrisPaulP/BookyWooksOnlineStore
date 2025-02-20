using BookWooks.OrderApi.Core.OrderAggregate.BusinessRuleErrors;
using BookyWooks.SharedKernel.Validation;

namespace BookWooks.OrderApi.Core.OrderAggregate.Entities;

public record OrderItem(
    OrderId OrderId,
    ProductId ProductId,
    Price Price,
    Quantity Quantity,
    ProductName ProductName,
    ProductDescription ProductDescription) : EntityBase
{
  public Validation<OrderItemValidationErrors, OrderItem> UpdateQuantity(int newQuantity)
  {
    var updatedQuantity = Quantity.TryFrom(newQuantity).ToValidationMonad(error => new OrderItemValidationErrors("quantity", error));

    return updatedQuantity.Apply((quantityVo) => this with { Quantity = (Quantity)quantityVo });
  }

  public Validation<OrderItemValidationErrors, OrderItem> UpdatePrice(int newPrice)
  {
    var updatedPrice = Price.TryFrom(newPrice).ToValidationMonad(error => new OrderItemValidationErrors("price", error));
    return updatedPrice.Apply((priceVo) => this with { Price = (Price)priceVo });
  }

    internal static Validation<OrderItemValidationErrors, OrderItem> AddOrderItem(
        Guid orderId, Guid productId, decimal price, int quantity, string productName, string productDescription)
    {
        var maybeOrderId = OrderId.TryFrom(orderId).ToValidationMonad(error => new OrderItemValidationErrors("orderId", error));
        var maybeProductId = ProductId.TryFrom(productId).ToValidationMonad(error => new OrderItemValidationErrors("productId", error));
        var maybePrice = Price.TryFrom(price).ToValidationMonad(error => new OrderItemValidationErrors("price", error));
        var maybeQuantity = Quantity.TryFrom(quantity).ToValidationMonad(error => new OrderItemValidationErrors("quantity", error));
        var maybeProductName = ProductName.TryFrom(productName).ToValidationMonad(error => new OrderItemValidationErrors("productName", error));
        var maybeProductDescription = ProductDescription.TryFrom(productDescription).ToValidationMonad(error => new OrderItemValidationErrors("productDescription", error));

        return (maybeOrderId, maybeProductId, maybePrice, maybeQuantity, maybeProductName, maybeProductDescription)
            .Apply((orderIdVo, productIdVo, priceVo, quantityVo, productNameVo, productDescriptionVo) =>
                new OrderItem(orderIdVo, productIdVo, priceVo, quantityVo, productNameVo, productDescriptionVo)
            );
    }
}


[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct OrderId
{
  private static Validation Validate(Guid input) {
    var result = Validation.Invalid("OrderId does not meet requirements");
    if (input == Guid.Empty) result.WithData(BusinessRuleError.Create("OrderId must be a valid GUID"), string.Empty);
    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }
}

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct ProductId
{
  private static Validation Validate(Guid input) {
    var result = Validation.Invalid("ProductId does not meet requirements");
    if (input == Guid.Empty) result.WithData(BusinessRuleError.Create("ProductId must be a valid GUID"), string.Empty);
    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }
}
[ValueObject<decimal>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct Price
{
  private static Validation Validate(decimal input)
  {
    var result = Validation.Invalid("Price does not meet requirements");
    if (input <= 0) result.WithData(BusinessRuleError.Create("Price must be greater than zero"), string.Empty);
    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }
}
[ValueObject<int>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct Quantity
{
  private static Validation Validate(int input)
  {
    var result = Validation.Invalid("Quantity does not meet requirements");
    if (input <= 0) result.WithData(BusinessRuleError.Create("Quantity must be greater than zero"), string.Empty);
    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct ProductName
{
  private static Validation Validate(string input)
  {
    var result = Validation.Invalid("ProductName does not meet requirements");
    if (string.IsNullOrWhiteSpace(input)) result.WithData(BusinessRuleError.Create("ProductName is required"), string.Empty);
    if (input.Length > 100) result.WithData(BusinessRuleError.Create("ProductName must be less than 100 characters"), string.Empty);
    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public partial record struct ProductDescription
{
  private static Validation Validate(string input)
  {
    var result = Validation.Invalid("ProductDescription does not meet requirements");
    if (string.IsNullOrWhiteSpace(input)) result.WithData(BusinessRuleError.Create("ProductDescription is required"), string.Empty);
    if (input.Length > 500) result.WithData(BusinessRuleError.Create("ProductDescription must be less than 500 characters"), string.Empty);
    return result.Data is { Count: > 0 } ? result : Validation.Ok;
  }
}
