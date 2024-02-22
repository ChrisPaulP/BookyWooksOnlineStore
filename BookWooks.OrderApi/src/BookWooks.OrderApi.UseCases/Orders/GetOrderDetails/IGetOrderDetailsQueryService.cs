using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.UseCases.Contributors;

namespace BookWooks.OrderApi.UseCases.Orders.Get;
public interface IGetOrderDetailsQueryService
{
  Task<IEnumerable<OrderDTO>> ListOrdersAsync();
}

