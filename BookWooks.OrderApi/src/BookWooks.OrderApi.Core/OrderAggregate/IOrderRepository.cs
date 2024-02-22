using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookyWooks.SharedKernel;

namespace BookWooks.OrderApi.Core.OrderAggregate;
public interface IOrderRepository : IRepository<Order, Guid>
{
  Task<Order?> FindOrderAsync(ISpecification<Order> specification);
  //Task<IEnumerable<Order>> GetOrders();
  //Task<Order?> GetOrderById(Guid orderId);
  //Task <Order> AddAsync(Order order);
  //void Update(Order order);
}
