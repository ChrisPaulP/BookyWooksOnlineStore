using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookWooks.OrderApi.Core.OrderAggregate.Enums;
using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
using Xunit;

namespace BookWooks.OrderApi.UnitTests.Core.OrderAggregate;
public class OrderConstructor
{
  private readonly string _testName = "Pending";
  public static readonly DeliveryAddress DeliveryAddress = DeliveryAddress.Of("Test Street", "Test City", "Test Country", "Test Post Code");
  public static readonly Payment PaymentDetails = Payment.Of("1234 5678 9012 3456", "Christopher", "12/23", "123", 1);
  public static readonly Order Order1 = Order.Create(Guid.NewGuid(), DeliveryAddress, PaymentDetails);

  private Order? _testOrder;

  private Order CreateOrder()
  {
    return Order1;
  }

  [Fact]
  public void InitializesName()
  {
    _testOrder = CreateOrder();

    Assert.Equal(_testName, _testOrder.Status.Name);
  }
}
