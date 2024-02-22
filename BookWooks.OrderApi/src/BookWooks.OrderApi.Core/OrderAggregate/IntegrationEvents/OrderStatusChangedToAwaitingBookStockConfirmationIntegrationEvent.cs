namespace BookWooks.OrderApi.Core.OrderAggregate.IntegrationEvents;
public record OrderStatusChangedToAwaitingBookStockConfirmationIntegrationEvent : IntegrationEventBase
{
  public string OrderStatus { get; }
  public string BuyerName { get; }
  public IEnumerable<OrderStockItem> OrderStockItems { get; }
  public OrderStatusChangedToAwaitingBookStockConfirmationIntegrationEvent(Guid orderId, string orderStatus, string buyerName,
        IEnumerable<OrderStockItem> orderStockItems)
  {
    OrderId = orderId;
    OrderStockItems = orderStockItems;
    OrderStatus = orderStatus;
    BuyerName = buyerName;

  }
  public Guid OrderId { get; init; }
}

public record OrderStockItem
{
  public int ProductBookId { get; }
  public int Quantity { get; }

  public OrderStockItem(int productBookId, int quantity)
  {
    ProductBookId = productBookId;
    Quantity = quantity;
  }
}
