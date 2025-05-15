using BookWooks.OrderApi.UseCases.Errors;
using LanguageExt;
using Microsoft.Data.SqlClient;

namespace BookWooks.OrderApi.Infrastructure.Data.Queries;
public class GetOrdersByStatusQueryService : IGetOrdersByStatusQueryService
{
  private readonly BookyWooksOrderDbContext _db;

  public GetOrdersByStatusQueryService(BookyWooksOrderDbContext db) => _db = db;

  public async Task<Either<OrderNotFound, IEnumerable<OrderWithItemsDTO>>> GetOrdersByStatusAsync(string status) =>
         (await _db.Orders
        .FromSqlRaw("SELECT * FROM Orders WHERE Status = @status", new SqlParameter("@status", status))
        .Include(b => b.OrderItems)
        .ToListAsync())
        .ToEither(() => new OrderNotFound())
        .Map(orders => orders.Select(OrderMappingExtensions.ToOrderWithItemsDTO)); 
}
