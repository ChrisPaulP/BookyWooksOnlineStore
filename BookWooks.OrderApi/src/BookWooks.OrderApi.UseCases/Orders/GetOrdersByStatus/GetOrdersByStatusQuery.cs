using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BookyWooks.SharedKernel;

namespace BookWooks.OrderApi.UseCases.Orders.List;
public record GetOrdersByStatusQuery(int? Skip, int? Take, string Status) : IQuery<Result<IEnumerable<OrderDTO>>>;
