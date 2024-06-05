using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
using BookWooks.OrderApi.UseCases.Create;

namespace BookWooks.OrderApi.Infrastructure.Data.Extensions;

public class InitialData
{
  private const string ChrisEmail = "chris@gmail.com";
  private const string RebeccaEmail = "rebecca@gmail.com";
  private const string HarryPotterTitle = "Harry Potter The Goblet of Fire";
  private const string TheStandTitle = "The Stand";
  private const string MagpieMurdersTitle = "Magpie Murders";

  public static readonly List<Customer> Customers = new()
  {
    Customer.Create("chris", ChrisEmail),
    Customer.Create("rebecca", RebeccaEmail),
  };

  public static readonly List<Product> Products = new()
  {
    Product.Create(HarryPotterTitle, "harrypotter.pic", 10),
    Product.Create(TheStandTitle, "stand.pic", 15),
    Product.Create(MagpieMurdersTitle, "magpiemurders.pic", 5),
  };

  public static IEnumerable<Order> OrdersWithItems => CreateOrders();

  private static IEnumerable<Order> CreateOrders()
  {
    var customers = Customers.ToList();
    var deliveryAddresses = CreateDeliveryAddresses();
    var payments = CreatePayments();

    var orders = new List<Order>
    {
      CreateOrder(ChrisEmail, deliveryAddresses[0], payments[0],
        new List<(string title, int quantity, int price)>
        {
          (HarryPotterTitle, 1, 1),
          (TheStandTitle, 1, 1)
        }),

      CreateOrder(RebeccaEmail, deliveryAddresses[0], payments[1],
        new List<(string title, int quantity, int price)>
        {
          (HarryPotterTitle, 1, 4),
          (TheStandTitle, 1, 2),
          (MagpieMurdersTitle, 1, 1)
        })
    };

    return orders;
  }

  private static List<DeliveryAddress> CreateDeliveryAddresses()
  {
    return new List<DeliveryAddress>
        {
            DeliveryAddress.Of("Test Street", "Test City", "Test Country", "Post Code"),
            DeliveryAddress.Of("Fiction Street", "Fiction City", "Fiction Country", "Post Code")
        };
  }

  private static List<Payment> CreatePayments()
  {
    return new List<Payment>
        {
            Payment.Of("chris", "5555555555554444", "12/28", "355", 1),
            Payment.Of("rebecca", "8885555555554444", "06/30", "222", 2)
        };
  }

  private static Order CreateOrder(string email, DeliveryAddress deliveryAddress, Payment payment, List<(string title, int quantity, int price)> items)
  {
    var customerId = GetCustomerIdByEmail(email);
    var order = Order.Create(customerId, deliveryAddress, payment);

    foreach (var item in items)
    {
      var productId = GetProductIdByTitle(item.title);
      order.AddOrderItem(productId, item.quantity, item.price);
    }

    return order;
  }

  private static Guid GetCustomerIdByEmail(string email)
  {
    return Customers.FirstOrDefault(x => x.Email == email)?.Id ?? Guid.NewGuid();
  }

  private static Guid GetProductIdByTitle(string title)
  {
    return Products.FirstOrDefault(x => x.Title == title)?.Id ?? Guid.NewGuid();
  }
}
