

using BookWooks.OrderApi.Core.OrderAggregate;
using Xunit;

namespace BookWooks.OrderApi.UnitTests.Core.OrderAggregate;
public class OrderConstructor
{
  private readonly string _testName = "Pending";
  public static readonly DeliveryAddress DeliveryAddress = new("Test Street", "Test City", "Test Country", "Test Post Code");
  public static readonly Order Order1 = new("UserIdNo", "Crispy", DeliveryAddress, "12345", "11111", "Christopher", OrderStatus.Pending);

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
