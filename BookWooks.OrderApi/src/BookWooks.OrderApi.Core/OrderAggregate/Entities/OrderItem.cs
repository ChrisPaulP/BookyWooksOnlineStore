namespace BookWooks.OrderApi.Core.OrderAggregate.Entities;

using BookWooks.OrderApi.Core.OrderAggregate.Events;

public record OrderItem : EntityBase
{
    private OrderItem(OrderItemId orderItemId, OrderId orderId, ProductId productId, ProductPrice price,
        ProductQuantity quantity, ProductName productName, ProductDescription productDescription)
    {
        OrderItemId = orderItemId;
        OrderId = orderId;
        ProductId = productId;
        Price = price;
        Quantity = quantity;
        ProductName = productName;
        ProductDescription = productDescription;
    }

    public OrderItemId OrderItemId { get; }
    public OrderId OrderId { get; }
    public ProductId ProductId { get; }
    public ProductPrice Price { get; }
    public ProductQuantity Quantity { get; }
    public ProductName ProductName { get; }
    public ProductDescription ProductDescription { get; }

    /// <summary>
    /// Updates the quantity of the order item.
    /// </summary>
    /// <param name="newQuantity">The new quantity to set</param>
    /// <returns>A validation result containing either the updated order item or validation errors</returns>
    public Validation<OrderItemValidationErrors, OrderItem> UpdateQuantity(int newQuantity)
    {
        const int MaxQuantity = 99;
        if (newQuantity > MaxQuantity)
        {
            return new OrderItemValidationErrors(
                DomainValidationMessages.ProductQuantity,
                [BusinessRuleError.Create($"Quantity cannot exceed {MaxQuantity}")]);
        }

        var quantityValidation = ProductQuantity.TryFrom(newQuantity)
            .ToValidationMonad(error => new OrderItemValidationErrors(DomainValidationMessages.ProductQuantity, error));

        return quantityValidation.Map(createdQuantity =>
        {
            var previousQuantity = Quantity;
            var updated = new OrderItem(
                OrderItemId, OrderId, ProductId, Price, 
                createdQuantity, ProductName, ProductDescription);
            updated.RegisterDomainEvent(new OrderItemQuantityUpdatedEvent(OrderItemId, createdQuantity, previousQuantity));
            return updated;
        });
    }

    /// <summary>
    /// Updates the price of the order item.
    /// </summary>
    /// <param name="newPrice">The new price to set</param>
    /// <returns>A validation result containing either the updated order item or validation errors</returns>
    public Validation<OrderItemValidationErrors, OrderItem> UpdatePrice(decimal newPrice)
    {
        const decimal MinPrice = 0.01m;
        if (newPrice < MinPrice)
        {
            return new OrderItemValidationErrors(
                DomainValidationMessages.ProductPrice,
                [BusinessRuleError.Create($"Price must be at least {MinPrice:C}")]);
        }

        var updatedPriceValidation = ProductPrice.TryFrom(newPrice)
            .ToValidationMonad(error => new OrderItemValidationErrors(DomainValidationMessages.ProductPrice, error));
            
        return updatedPriceValidation.Map(createdPrice => 
            new OrderItem(OrderItemId, OrderId, ProductId, createdPrice, 
                Quantity, ProductName, ProductDescription));
    }

    /// <summary>
    /// Creates a new order item with validation.
    /// </summary>
    internal static Validation<OrderItemValidationErrors, OrderItem> AddOrderItem(
        Guid orderId, Guid productId, decimal price, int quantity, string productName, string productDescription)
    {
        var orderIdValidation = OrderId.TryFrom(orderId)
            .ToValidationMonad(errors => new OrderItemValidationErrors(DomainValidationMessages.OrderId, errors));
        var productIdValidation = ProductId.TryFrom(productId)
            .ToValidationMonad(errors => new OrderItemValidationErrors(DomainValidationMessages.ProductId, errors));
        var priceValidation = ProductPrice.TryFrom(price)
            .ToValidationMonad(errors => new OrderItemValidationErrors(DomainValidationMessages.ProductPrice, errors));
        var quantityValidation = ProductQuantity.TryFrom(quantity)
            .ToValidationMonad(errors => new OrderItemValidationErrors(DomainValidationMessages.ProductQuantity, errors));
        var productNameValidation = ProductName.TryFrom(productName)
            .ToValidationMonad(errors => new OrderItemValidationErrors(DomainValidationMessages.ProductName, errors));
        var productDescriptionValidation = ProductDescription.TryFrom(productDescription)
            .ToValidationMonad(errors => new OrderItemValidationErrors(DomainValidationMessages.ProductDescription, errors));

        return (orderIdValidation, productIdValidation, priceValidation, quantityValidation, 
                productNameValidation, productDescriptionValidation)
            .Apply((createdOrderId, createdProductId, createdPrice, createdQuantity, 
                    createdProductName, createdProductDescription) =>
                new OrderItem(OrderItemId.New(), createdOrderId, createdProductId, createdPrice,      
                    createdQuantity, createdProductName, createdProductDescription));
    }
}
