namespace BookWooks.OrderApi.Core.OrderAggregate.Enums;
public class OrderStatus : SmartEnum<OrderStatus>
{
  public static readonly OrderStatus Pending = new(nameof(Pending), 1);
  public static readonly OrderStatus Approved = new(nameof(Approved), 2);
  public static readonly OrderStatus Cancelled = new(nameof(Cancelled), 3);
  public static readonly OrderStatus OrderCheckingBookStock = new(nameof(OrderCheckingBookStock), 4);
  public static readonly OrderStatus BookStockConfirmed = new(nameof(BookStockConfirmed), 5);
  public static readonly OrderStatus Fulfilled = new(nameof(Fulfilled), 6);

  protected OrderStatus(string name, int value) : base(name, value) { }
}

