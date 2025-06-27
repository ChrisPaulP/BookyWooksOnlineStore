namespace BookWooks.OrderApi.Core.OrderAggregate.Enums;

public abstract class OrderStatus
{
  public static readonly OrderStatus NewOrderCreated = new NewOrderStatus();
  public static readonly OrderStatus Pending = new PendingStatus();
  public static readonly OrderStatus Approved = new ApprovedStatus();
  public static readonly OrderStatus Cancelled = new CancelledStatus();
  public static readonly OrderStatus OrderCheckingBookStock = new OrderCheckingBookStockStatus();
  public static readonly OrderStatus BookStockConfirmed = new BookStockConfirmedStatus();
  public static readonly OrderStatus Fulfilled = new FulfilledStatus();
  public static readonly OrderStatus Delivered = new DeliveredStatus();

  public abstract string Label { get; }

  private class NewOrderStatus : OrderStatus
  {
    public override string Label => "New Order Created";
  }
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
      "New Order Created" => NewOrderCreated,
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


