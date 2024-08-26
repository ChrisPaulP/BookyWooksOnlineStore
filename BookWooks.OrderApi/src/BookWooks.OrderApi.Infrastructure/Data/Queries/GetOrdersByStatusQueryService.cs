namespace BookWooks.OrderApi.Infrastructure.Data.Queries;
public class GetOrdersByStatusQueryService : IGetOrdersByStatusQueryService
{
  private readonly BookyWooksOrderDbContext _db;

  public GetOrdersByStatusQueryService(BookyWooksOrderDbContext db)
  {
    _db = db;
  }
  public async Task<IEnumerable<OrderDTO>> GetOrdersByStatusAsync(string status)
  {
   
    var result =  await _db.Orders
        .FromSqlRaw($"SELECT Id, Status FROM Orders")
        .Include(b => b.OrderItems)
        .Select(o => new OrderDTO(
          o.Id,
          o.Status.Label,
          o.OrderItems.Select(item => new OrderItemDTO(item.ProductId, item.Price, item.Quantity)).ToList()
      ))
        .ToListAsync();

    return result;
  }
}

