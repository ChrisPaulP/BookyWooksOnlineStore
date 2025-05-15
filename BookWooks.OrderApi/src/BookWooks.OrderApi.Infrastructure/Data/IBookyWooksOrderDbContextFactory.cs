namespace BookWooks.OrderApi.Infrastructure.Data;

  public interface IBookyWooksOrderDbContextFactory
  {
    BookyWooksOrderDbContext CreateDbContext();
  }
public class BookyWooksOrderDbContextFactory : IBookyWooksOrderDbContextFactory
{
  private readonly IServiceProvider _serviceProvider;

  public BookyWooksOrderDbContextFactory(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
  }

  public BookyWooksOrderDbContext CreateDbContext()
  {
    return _serviceProvider.GetRequiredService<BookyWooksOrderDbContext>();
  }
}
