using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
using BookWooks.OrderApi.Infrastructure.Data;

namespace BookWooks.OrderApi.FunctionalTests;
internal static class TestData
{
  public static readonly DeliveryAddress DeliveryAddress = DeliveryAddress.Of("Test Street", "Test City", "Test Country", "Post Code");
  public static readonly Payment PaymentDetails = Payment.Of("1234 5678 9012 3456", "Christopher", "12/23", "123", 1);
  public static readonly Customer Customer1 = Customer.Create("Customer Name", "Email Address");
  public static readonly Order Order1 = Order.Create(Customer1.Id, DeliveryAddress, PaymentDetails);

  public static void PopulateTestData(BookyWooksOrderDbContext dbContext)
  {
    dbContext.Customers.RemoveRange(dbContext.Customers);
    dbContext.Orders.RemoveRange(dbContext.Orders);

    dbContext.Customers.Add(Customer1);
    dbContext.Orders.Add(Order1);

    dbContext.SaveChanges();
  }
}
