using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;

namespace BookWooks.OrderApi.Web;
public static class SeedData
{

  public static readonly DeliveryAddress DeliveryAddress = DeliveryAddress.Of("Test Street", "Test City", "Test Country", "Post Code");
  public static readonly Payment PaymentDetails = Payment.Of("1234 5678 9012 3456", "Christopher", "12/23", "123", 1);
  

  
  public static readonly Customer Customer1 = Customer.Create("Customer Name", "Email Address");
  public static readonly Order Order1 = Order.Create(Customer1.Id, DeliveryAddress, PaymentDetails);



  public static void Initialize(IServiceProvider serviceProvider)
  {
    using (var dbContext = new BookyWooksOrderDbContext(
        serviceProvider.GetRequiredService<DbContextOptions<BookyWooksOrderDbContext>>(), null)) 
    {
      
      // Look for any Contributors.
      if (dbContext.Orders.Any())
      {
        return;   // DB has been seeded
      }

      PopulateTestData(dbContext);
    }
  }
  public static void PopulateTestData(BookyWooksOrderDbContext dbContext)
  {
    foreach (var customer in dbContext.Customers)
    {
      dbContext.Remove(customer);
    }

    dbContext.Customers.Add(Customer1);
    foreach (var item in dbContext.Orders)
    {
      dbContext.Remove(item);
    }
    dbContext.SaveChanges();

    dbContext.Orders.Add(Order1);

   

    dbContext.SaveChanges();
  }
}
