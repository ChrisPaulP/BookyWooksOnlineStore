using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BookWooks.OrderApi.UseCases.Contributors;
using BookyWooks.SharedKernel;

namespace BookWooks.OrderApi.UseCases.Orders.Get;
public record GetOrderDetailsQuery(Guid OrderId) : IQuery<Result<OrderDTO>>;

