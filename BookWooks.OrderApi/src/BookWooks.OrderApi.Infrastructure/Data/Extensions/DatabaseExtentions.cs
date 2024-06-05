
using Microsoft.AspNetCore.Builder;

namespace BookWooks.OrderApi.Infrastructure.Data.Extensions;
public static class DatabaseExtentions
{
  public static async Task InitialiseDatabaseAsync(this WebApplication app)
  {
    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<BookyWooksOrderDbContext>();

    await context.Database.MigrateAsync();

    await SeedAsync(context);
  }

  private static async Task SeedAsync(BookyWooksOrderDbContext context)
  {
    await SeedCustomerAsync(context);
    await SeedProductAsync(context);
    await SeedOrdersWithItemsAsync(context);
  }

  private static async Task SeedCustomerAsync(BookyWooksOrderDbContext context)
  {
    if (!await context.Customers.AnyAsync())
    {
      await context.Customers.AddRangeAsync(InitialData.Customers);
      await context.SaveChangesAsync();
    }
  }

  private static async Task SeedProductAsync(BookyWooksOrderDbContext context)
  {
    if (!await context.Products.AnyAsync())
    {
      await context.Products.AddRangeAsync(InitialData.Products);
      await context.SaveChangesAsync();
    }
  }

  private static async Task SeedOrdersWithItemsAsync(BookyWooksOrderDbContext context)
  {
    if (!await context.Orders.AnyAsync())
    {
      await context.Orders.AddRangeAsync(InitialData.OrdersWithItems);
      await context.SaveChangesAsync();
    }
  }
}
