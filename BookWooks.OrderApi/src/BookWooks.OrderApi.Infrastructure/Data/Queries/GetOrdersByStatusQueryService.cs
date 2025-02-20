namespace BookWooks.OrderApi.Infrastructure.Data.Queries;
public class GetOrdersByStatusQueryService : IGetOrdersByStatusQueryService
{
  private readonly BookyWooksOrderDbContext _db;

  public GetOrdersByStatusQueryService(BookyWooksOrderDbContext db)
  {
    _db = db;
  }
  public async Task<IEnumerable<OrderWithItemsDTO>> GetOrdersByStatusAsync(string status)
  {

    return await _db.Orders
        .FromSqlRaw("SELECT Id, Status FROM Orders WHERE Status = {0}", status)
        .Include(b => b.OrderItems)
        .Select(o => new OrderWithItemsDTO(
          o.Id,
          o.Status.Label,
          o.OrderItems.Select(item => new OrderItemDTO(item.ProductId.Value, item.ProductName.Value, item.ProductDescription.Value, item.Price.Value, item.Quantity.Value)).ToList()
      ))
        .ToListAsync();
  }
}

