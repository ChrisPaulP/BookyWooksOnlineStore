using Newtonsoft.Json.Linq;

namespace BookWooks.OrderApi.Core.OrderAggregate.Entities;
public class OrderItem : EntityBase
{
  public decimal Price { get; private set; }
  public string Message { get; private set; }
  public int Quantity { get; private set; }
  public Guid OrderId { get; private set; }
  public Guid ProductId { get; private set; }
  protected OrderItem()
  {
    Message = "New Order Item Added";
  }

  public OrderItem(Guid orderId, Guid productId, decimal price, int quantity = 1) : this()
  {
    ArgumentNullException.ThrowIfNull(orderId);
    ArgumentNullException.ThrowIfNull(productId);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
    Price = price;
    Quantity = quantity;
    OrderId = orderId;
    ProductId = productId;
  }
  public void AddToQuantity(int quantity)
  {
    if (quantity < 0)
    {
      //throw new GenericOrderingDomainException("Invalid units");
    }

    Quantity += quantity;
  }
  public void RemoveFromQuantity(int quantity)
  {
    if (quantity < 0)
    {
      // throw new GenericOrderingDomainException("There are no Items to remove");
    }

    Quantity -= quantity;
  }
}
