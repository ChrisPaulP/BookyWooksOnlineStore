namespace BookWooks.OrderApi.Core.OrderAggregate.Enums;
//public class OrderStatus : SmartEnum<OrderStatus>
//{
//  public static readonly OrderStatus Pending = new(nameof(Pending), 1);
//  public static readonly OrderStatus Approved = new(nameof(Approved), 2);
//  public static readonly OrderStatus Cancelled = new(nameof(Cancelled), 3);
//  public static readonly OrderStatus OrderCheckingBookStock = new(nameof(OrderCheckingBookStock), 4);
//  public static readonly OrderStatus BookStockConfirmed = new(nameof(BookStockConfirmed), 5);
//  public static readonly OrderStatus Fulfilled = new(nameof(Fulfilled), 6);

//  protected OrderStatus(string name, int value) : base(name, value) { }
//}

public abstract class OrderStatus
{
  public static readonly OrderStatus Pending = new PendingStatus();
  public static readonly OrderStatus Approved = new ApprovedStatus();
  public static readonly OrderStatus Cancelled = new CancelledStatus();
  public static readonly OrderStatus OrderCheckingBookStock = new OrderCheckingBookStockStatus();
  public static readonly OrderStatus BookStockConfirmed = new BookStockConfirmedStatus();
  public static readonly OrderStatus Fulfilled = new FulfilledStatus();
  public static readonly OrderStatus Delivered = new DeliveredStatus();

  public abstract string Label { get; }

  private class PendingStatus : OrderStatus
  {
    public override string Label => "Pending";
  }

  private class ApprovedStatus : OrderStatus
  {
    public override string Label => "Approved";
  }

  private class CancelledStatus : OrderStatus
  {
    public override string Label => "Cancelled";
  }

  private class OrderCheckingBookStockStatus : OrderStatus
  {
    public override string Label => "Order Checking Book Stock";
  }

  private class BookStockConfirmedStatus : OrderStatus
  {
    public override string Label => "Book Stock Confirmed";
  }

  private class FulfilledStatus : OrderStatus
  {
    public override string Label => "Fulfilled";
  }

  private class DeliveredStatus : OrderStatus
  {
    public override string Label => "Delivered";
  }

  public static OrderStatus FromLabel(string label)
  {
    return label switch
    {
      "Pending" => Pending,
      "Approved" => Approved,
      "Cancelled" => Cancelled,
      "Order Checking Book Stock" => OrderCheckingBookStock,
      "Book Stock Confirmed" => BookStockConfirmed,
      "Fulfilled" => Fulfilled,
      "Delivered" => Delivered,
      _ => throw new ArgumentException($"Unknown status: {label}")
    };
  }
}


