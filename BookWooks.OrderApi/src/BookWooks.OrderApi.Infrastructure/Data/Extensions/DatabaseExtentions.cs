using System.Diagnostics;
using System.Net;

namespace BookWooks.OrderApi.Infrastructure.Data.Extensions;
public static class DatabaseExtentions
{
  public static async Task InitialiseDatabaseAsync(this WebApplication app)
  {
    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<BookyWooksOrderDbContext>();
    //if (context.Database.CanConnect())
    //{
    //  await context.Database.EnsureDeletedAsync();
    //}
    await context.Database.MigrateAsync();
    await ClearData(context);
    await SeedAsync(context);
  }
  public static async Task ClearData(BookyWooksOrderDbContext context)
  {
    context.Orders.RemoveRange(context.Orders);
    context.Products.RemoveRange(context.Products);
    context.Customers.RemoveRange(context.Customers);
    context.InboxMessages.RemoveRange(context.InboxMessages);
    context.InternalCommands.RemoveRange(context.InternalCommands);
    context.OutboxMessages.RemoveRange(context.OutboxMessages);
    await context.SaveChangesAsync();
  }
  public static async Task SeedAsync(BookyWooksOrderDbContext context)
  {
    await SeedCustomers(context);
    await SeedProducts(context);
    await SeedOrders(context);
  }

  private static async Task SeedProducts(BookyWooksOrderDbContext context)
  {
    if (!await context.Products.AnyAsync())
    {
      await context.Products.AddRangeAsync(SeedData.ProductSeedData.CreateProducts());
      await context.SaveChangesAsync();
    }
  }
  private static async Task SeedCustomers(BookyWooksOrderDbContext context)
  {
    if (!await context.Customers.AnyAsync())
    {
      var customers =  SeedData.CustomerSeedData.CreateCustomers();
      await context.Customers.AddRangeAsync(customers);
      //await context.Customers.AddRangeAsync(SeedData.CustomerSeedData.CreateCustomers());
      await context.SaveChangesAsync();
    }
  }
    private static async Task SeedOrders(BookyWooksOrderDbContext context)
    {
        if (!await context.Orders.AnyAsync())
        {
      //var customerId = await context.Customers.Select(c => c.CustomerId.Value).DefaultIfEmpty(CustomerId.New().Value).FirstAsync();
      var customerIds = await context.Customers
      .Select(c => c.CustomerId.Value)
      .ToListAsync();
      var customerId = customerIds.DefaultIfEmpty(CustomerId.New().Value).FirstOrDefault();
      var product = await context.Products.FirstOrDefaultAsync();
            if (product != null)
            {
        //await context.Orders.AddRangeAsync(SeedData.OrderSeedData.CreateOrders(customerId, product));
        //      var orders = SeedData.OrderSeedData.CreateOrders(customerId, product);
        //      foreach (var order in orders)
        //      {
        //        Debug.Assert(order.OrderId.Value != default, "OrderId should not be default GUID!");
        //        Debug.Assert(order.CustomerId.Value != default, "CustomerId should not be default GUID!");
        //        Debug.Assert(order.DeliveryAddress != null, "DeliveryAddress should not be null!");
        //        Debug.Assert(order.Payment != null, "Payment should not be null!");
        //        Debug.Assert(order.Status != null, "Status should not be null!");
        //        Debug.Assert(order.OrderPlaced.IsInitialized());
        //        Debug.Assert(order.Message != null, "Message should not be null!");
        //        Debug.Assert(order.OrderItems != null, "OrderItems should not be null!");
        //        Debug.Assert(order.OrderId.Value != default, "OrderId should not be default GUID!");
        //      }

        //foreach (var order in orders)
        //{
        //  Console.WriteLine($"🛠 Before Save: OrderId = {order.OrderId.Value}");
        //  Debug.Assert(order.OrderId.IsInitialized(), "❌ OrderId is uninitialized before saving!");
        //}

        //foreach (var entry in context.ChangeTracker.Entries<Order>())
        //{
        //  Console.WriteLine($"🔍 Tracking Order: {entry.Entity.OrderId.Value}");
        //}

        try
        {
          //var order = orders.First();

          var testOrder = Order.CreateOrder(customerId, "123 Test St", "Testville", "Testland", "TST123", "Test User", "4111111111111111", "12/30", 1);
          var myOrder = (Order)testOrder;
          
          await context.Orders.AddAsync(myOrder);

          //await context.Orders.AddRangeAsync(order);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"🔥 Exception: {ex.Message}");
          Console.WriteLine($"📜 Stack Trace: {ex.StackTrace}");
          if (ex.InnerException != null)
          {
            Console.WriteLine($"🔍 Inner Exception: {ex.InnerException.Message}");
          }
          throw;
        }
        await context.SaveChangesAsync();
            }

        }
    }
}
