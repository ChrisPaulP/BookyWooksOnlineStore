using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using BookWooks.OrderApi.Core.OrderAggregate;
using BookyWooks.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace BookWooks.OrderApi.Infrastructure.Data.Repositories;
public class OrderRepository : IOrderRepository
{
  private readonly BookyWooksOrderDbContext _orderDbContext;
  public OrderRepository(BookyWooksOrderDbContext orderDbContext)
  {
    _orderDbContext = orderDbContext;
  }
  public IUnitOfWork UnitOfWork => _orderDbContext;

  public async Task<IEnumerable<Order>> GetOrders()
  {
    return await _orderDbContext.Orders.Include(oi => oi.OrderItems).OrderBy(o => o.OrderPlaced).ToListAsync();
  }

  public async Task<Order?> GetOrderById(Guid orderId)
  {
    var order = await _orderDbContext
                           .Orders
                           .Include(x => x.DeliveryAddress)
                           .FirstOrDefaultAsync(o => o.Id == orderId);
    if (order == null)
    {
      order = _orderDbContext
                  .Orders
                  .Local
                  .FirstOrDefault(o => o.Id == orderId);
    }
    if (order != null)
    {
      await _orderDbContext.Entry(order)
          .Collection(i => i.OrderItems).LoadAsync();
    }
    return order;
  }
  public async Task<Order> AddAsync(Order order)
  {
     var addedOrder = await _orderDbContext.Orders.AddAsync(order);
     return addedOrder.Entity;
  }
  
  public void Update(Order order)
  {
    _orderDbContext.Entry(order).State = EntityState.Modified;
  }

  public async Task<Order?> TFindById(Guid id)
  {
    var order = _orderDbContext.Orders.Local.FirstOrDefault(o => o.Id == id);

    if (order == null)
    {
      order = await _orderDbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);
    }

    if (order != null)
    {
      await _orderDbContext.Entry(order)
          .Collection(i => i.OrderItems)
          .LoadAsync();
    }

    return order;
  }

  public async Task<Order?> FindOrderAsync(ISpecification<Order> specification) //public async Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification).FirstOrDefaultAsync();
  }
  public async Task<List<Order>> FindAsync(ISpecification<Order> specification) //public async Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
  {
    return  await ApplySpecification(specification).ToListAsync();
  }

  public void AddRange(IEnumerable<Order> entities)
  {
     _orderDbContext.Set<Order>().AddRange(entities);
  }

  public void Remove(Order entity)
  {
    _orderDbContext.Set<Order>().Remove(entity);
  }

  public void RemoveRange(IEnumerable<Order> entities)
  {
    _orderDbContext.Set<Order>().RemoveRange(entities);
  }

  public bool Contains(ISpecification<Order> specification)
  {
    return Count(specification) > 0 ? true : false;
  }

  public bool Contains(Expression<Func<Order, bool>> predicate)
  {
    return Count(predicate) > 0 ? true : false;
  }

  public int Count(ISpecification<Order> specification)
  {
    return ApplySpecification(specification).Count();
  }

  public int Count(Expression<Func<Order, bool>> predicate)
  {
    return _orderDbContext.Set<Order>().Where(predicate).Count();
  }
  private IQueryable<Order> ApplySpecification(ISpecification<Order> spec)
  {
    return SpecificationEvaluator<Order, Guid>.GetQuery(_orderDbContext.Set<Order>().AsQueryable(), spec);
  }
}
