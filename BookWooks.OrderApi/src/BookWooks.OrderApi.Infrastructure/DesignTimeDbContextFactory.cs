namespace BookWooks.OrderApi.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BookyWooksOrderDbContext>
{
  public BookyWooksOrderDbContext CreateDbContext(string[] args)
  {

    var basePath = Directory.GetCurrentDirectory(); 
    if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
    {
      basePath = Path.GetFullPath(Path.Combine(basePath, "..", "BookWooks.OrderApi.Web"));
    }

    var configuration = new ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddJsonFile("appsettings.json")
        .Build();

    var optionsBuilder = new DbContextOptionsBuilder<BookyWooksOrderDbContext>();
    optionsBuilder.UseSqlServer(configuration.GetConnectionString("OrderDatabase"));

    return new BookyWooksOrderDbContext(optionsBuilder.Options);
  }
}
