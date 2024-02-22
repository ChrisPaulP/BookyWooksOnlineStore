namespace BookWooks.OrderApi.Web;
public static class SeedData
{

  public static readonly DeliveryAddress DeliveryAddress = new("Test Street", "Test City", "Test Country", "Test Post Code");
  
  public static readonly Order Order1 = new( "UserIdNo", "Crispy", DeliveryAddress, "12345", "11111", "Christopher", OrderStatus.Pending);

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
    foreach (var item in dbContext.Orders)
    {
      dbContext.Remove(item);
    }
    dbContext.SaveChanges();

    dbContext.Orders.Add(Order1);

    dbContext.SaveChanges();
  }
}
