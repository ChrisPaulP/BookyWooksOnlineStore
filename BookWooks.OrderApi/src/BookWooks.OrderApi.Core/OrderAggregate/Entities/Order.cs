using System;
using System.Collections.Generic;
using System.Net;
using BookWooks.OrderApi.Core.OrderAggregate.Enums;
using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;

namespace BookWooks.OrderApi.Core.OrderAggregate.Entities;

public class Order : EntityBase, IAggregateRoot
{
  private readonly List<OrderItem> _orderItems = new();
  public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();
  public OrderStatus Status { get; private set; } = OrderStatus.Pending;
  public decimal OrderTotal => CalculateOrderTotal();
  public DateTimeOffset OrderPlaced { get; private set; }
  public bool OrderPaid { get; private set; }
  public bool IsCancelled { get; private set; }
  public string Message { get; private set; } = string.Empty;
  public Payment Payment { get; private set; } = default!;
  public DeliveryAddress DeliveryAddress { get; private set; } = default!;
  public Guid CustomerId { get; private set; }

  public static Order Create(Guid customerId, DeliveryAddress deliveryAddress, Payment payment) 
  {
    var order = new Order
    {
      CustomerId = customerId,
      DeliveryAddress = deliveryAddress,
      Payment = payment,
      Status = OrderStatus.Pending,
      Message = "New Order Created",
      IsCancelled = false
    };

    order.RegisterDomainEvent(new OrderCreatedEvent(order));

    return order;
  }

  public void AddOrderItem(Guid productId, decimal price, int quantity = 1)
  {
    var orderItem = new OrderItem(Id, productId, price, quantity);
    _orderItems.Add(orderItem);
  }

  private decimal CalculateOrderTotal()
  {
    decimal total = 0;

    foreach (var item in _orderItems)
    {
      total += item.Price * item.Quantity;
    }

    return total;
  }

  public void UpdateMessage(string newMessage)
  {
    Message = !string.IsNullOrEmpty(newMessage) ? newMessage : throw new ArgumentNullException(nameof(newMessage));
  }

  public void ApproveOrder()
  {
    Status = OrderStatus.Approved;
  }

  public void CancelOrder()
  {
    Status = OrderStatus.Cancelled;
  }

  public void ConfirmBookStock()
  {
    Status = OrderStatus.BookStockConfirmed;
  }

  public void FulfillOrder()
  {
    Status = OrderStatus.Fulfilled;
    RegisterDomainEvent(new OrderFulfilledEvent(this));
  }
}
