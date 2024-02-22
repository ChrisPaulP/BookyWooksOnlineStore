using System;
using System.Collections.Generic;
using System.Linq;
using BookWooks.OrderApi.Core.OrderAggregate;
using BookWooks.OrderApi.UseCases.Orders;
using BookWooks.OrderApi.UseCases.Orders.List;
using Microsoft.EntityFrameworkCore;
using Polly;

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
          o.Status.Name,
          o.OrderItems.Select(item => new OrderItemDTO(item.BookId, item.BookTitle, item.BookPrice, item.Quantity)).ToList()
      ))
        .ToListAsync();

    return result;
  }
}

