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
  private const string ToKill = "To Kill a Mockingbird";
  private const string Nineteen = "1984";
  private const string Pride = "Pride and Prejudice";
  public static readonly List<Customer> Customers = new()
  {
    Customer.Create("chris", ChrisEmail),
    Customer.Create("rebecca", RebeccaEmail),
  };

  public static readonly List<Product> Products = new()
  {
    Product.Create(new Guid("1e9c1a7e-1d9b-4c0e-8a15-5e12b5f5ad34"),ToKill, "to-kill-a-mockingbird.png", 10.99M),
    Product.Create(new Guid("2d65ff2a-c57a-44c8-8e49-51af4e276f68"),Nineteen, "1984.png", 8.99M),
    Product.Create(new Guid("3c4e6b45-738f-4a9a-85f5-68e26b3a58f9"),Pride, "pride-and-prejudice.png", 9.99M),
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
        new List<(string title, int quantity, decimal price)>
        {
          (ToKill, 1, 10.99M),
          (Nineteen, 1, 8.99M)
        }),

      CreateOrder(RebeccaEmail, deliveryAddresses[1], payments[1],
        new List<(string title, int quantity, decimal price)>
        {
          (ToKill, 1, 10.99M),
          (Nineteen, 1, 8.99M),
          (Pride, 1, 9.99M)
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

  private static Order CreateOrder(string email, DeliveryAddress deliveryAddress, Payment payment, List<(string title, int quantity, decimal price)> items)
  {
    var customerId = GetCustomerIdByEmail(email);
    var order = Order.Create(customerId, deliveryAddress, payment);

    foreach (var item in items)
    {
      var productId = GetProductIdByTitle(item.title);
      order.AddOrderItem(productId, item.price, item.quantity);
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
