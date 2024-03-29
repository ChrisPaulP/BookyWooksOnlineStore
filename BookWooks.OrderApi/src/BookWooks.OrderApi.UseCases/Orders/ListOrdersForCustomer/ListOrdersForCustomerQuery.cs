using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookyWooks.SharedKernel;

namespace BookWooks.OrderApi.UseCases.Orders.ListOrdersForCustomer;
public record ListOrdersForCustomerQuery(int? Skip, int? Take, Guid CustomerId) : IQuery<Result<IEnumerable<OrderDTO>>>;
