using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.UseCases.Orders;
using BookWooks.OrderApi.UseCases.Orders.List;

namespace BookWooks.OrderApi.Infrastructure.Data.Queries;
public class FakeGetOrdersByStatusQueryService : IGetOrdersByStatusQueryService
{
  public async Task<IEnumerable<OrderDTO>> GetOrdersByStatusAsync(string status)
  {
    var testProject = new OrderDTO(Guid.NewGuid(), "Test Order Status", null);
    return await Task.FromResult(new List<OrderDTO> { testProject });
  }
}
