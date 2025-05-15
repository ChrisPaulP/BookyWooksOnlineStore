using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;

namespace BookWooks.OrderApi.Core.OrderAggregate.Entities;
//public class OrderItem : EntityBase
//{
//  public OrderItemId OrderItemId { get; private set; }
//  public OrderId OrderId { get; private set; }
//  public ProductId ProductId { get; private set; }
//  public ProductPrice Price { get; private set; }
//  public ProductQuantity Quantity { get; private set; }
//  public ProductName ProductName { get; private set; }
//  public ProductDescription ProductDescription { get; private set; }

//  public OrderItem(OrderItemId orderItemId, OrderId orderId, ProductId productId, ProductPrice price, ProductQuantity quantity, ProductName productName, ProductDescription productDescription)
//  {
//    OrderItemId = orderItemId;
//    OrderId = orderId;
//    ProductId = productId;
//    Price = price;
//    Quantity = quantity;
//    ProductName = productName;
//    ProductDescription = productDescription;
//  }

//  public Validation<OrderItemValidationErrors, OrderItem> UpdateQuantity(int newQuantity)
//  {
//    var updatedQuantity = ProductQuantity.TryFrom(newQuantity).ToValidationMonad(error => new OrderItemValidationErrors("quantity", error));

//    return updatedQuantity.Apply(quantityVo =>
//    {
//      Quantity = (ProductQuantity)quantityVo;
//      return this;
//    });
//  }

//  public Validation<OrderItemValidationErrors, OrderItem> UpdatePrice(int newPrice)
//  {
//    var updatedPrice = ProductPrice.TryFrom(newPrice).ToValidationMonad(error => new OrderItemValidationErrors("price", error));
//    return updatedPrice.Apply(priceVo =>
//    {
//      Price = (ProductPrice)priceVo;
//      return this;
//    });
//  }

//  internal static Validation<OrderItemValidationErrors, OrderItem> AddOrderItem(
//      Guid orderId, Guid productId, decimal price, int quantity, string productName, string productDescription)
//  {
//    var maybeOrderId = OrderId.TryFrom(orderId).ToValidationMonad(errors => new OrderItemValidationErrors("orderId", errors));
//    var maybeProductId = ProductId.TryFrom(productId).ToValidationMonad(errors => new OrderItemValidationErrors("productId", errors));
//    var maybePrice = ProductPrice.TryFrom(price).ToValidationMonad(errors => new OrderItemValidationErrors("price", errors));
//    var maybeQuantity = ProductQuantity.TryFrom(quantity).ToValidationMonad(errors => new OrderItemValidationErrors("quantity", errors));
//    var maybeProductName = ProductName.TryFrom(productName).ToValidationMonad(errors => new OrderItemValidationErrors("productName", errors));
//    var maybeProductDescription = ProductDescription.TryFrom(productDescription).ToValidationMonad(errors => new OrderItemValidationErrors("productDescription", errors));

//    return (maybeOrderId, maybeProductId, maybePrice, maybeQuantity, maybeProductName, maybeProductDescription)
//        .Apply((orderIdVo, productIdVo, priceVo, quantityVo, productNameVo, productDescriptionVo) =>
//            new OrderItem(OrderItemId.New(), orderIdVo, productIdVo, priceVo, quantityVo, productNameVo, productDescriptionVo)
//        );
//  }
//}

public record OrderItem(
    OrderItemId OrderItemId,
    OrderId OrderId,
    ProductId ProductId,
    ProductPrice Price,
    ProductQuantity Quantity,
    ProductName ProductName,
    ProductDescription ProductDescription) : EntityBase
{

  public Validation<OrderItemValidationErrors, OrderItem> UpdateQuantity(int newQuantity)
  {
    var updatedQuantity = ProductQuantity.TryFrom(newQuantity).ToValidationMonad(error => new OrderItemValidationErrors("quantity", error));

    return updatedQuantity.Apply((quantityVo) => this with { Quantity = (ProductQuantity)quantityVo });
  }

  public Validation<OrderItemValidationErrors, OrderItem> UpdatePrice(int newPrice)
  {
    var updatedPrice = ProductPrice.TryFrom(newPrice).ToValidationMonad(error => new OrderItemValidationErrors("price", error));
    return updatedPrice.Apply((priceVo) => this with { Price = (ProductPrice)priceVo });
  }

  internal static Validation<OrderItemValidationErrors, OrderItem> AddOrderItem(
      Guid orderId, Guid productId, decimal price, int quantity, string productName, string productDescription)
  {
    var maybeOrderId = OrderId.TryFrom(orderId).ToValidationMonad(errors => new OrderItemValidationErrors("orderId", errors));
    var maybeProductId = ProductId.TryFrom(productId).ToValidationMonad(errors => new OrderItemValidationErrors("productId", errors));
    var maybePrice = ProductPrice.TryFrom(price).ToValidationMonad(errors => new OrderItemValidationErrors("price", errors));
    var maybeQuantity = ProductQuantity.TryFrom(quantity).ToValidationMonad(errors => new OrderItemValidationErrors("quantity", errors));
    var maybeProductName = ProductName.TryFrom(productName).ToValidationMonad(errors => new OrderItemValidationErrors("productName", errors));
    var maybeProductDescription = ProductDescription.TryFrom(productDescription).ToValidationMonad(errors => new OrderItemValidationErrors("productDescription", errors));

    return (maybeOrderId, maybeProductId, maybePrice, maybeQuantity, maybeProductName, maybeProductDescription)
        .Apply((orderIdVo, productIdVo, priceVo, quantityVo, productNameVo, productDescriptionVo) =>
            new OrderItem(OrderItemId.New(), orderIdVo, productIdVo, priceVo, quantityVo, productNameVo, productDescriptionVo)
        );
  }
}
