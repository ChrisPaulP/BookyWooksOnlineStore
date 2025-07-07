using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookyWooks.SharedKernel.Specification;

namespace BookWooks.OrderApi.Core.OrderAggregate.Specifications;
public class OrderByCustomerIdSpec : BaseSpecification<Order>
{
  public OrderByCustomerIdSpec(CustomerId customerId)
       : base(x => x.CustomerId == customerId)
  {
    AddInclude(Order.OrderItemsFieldName);
    EnableCache(nameof(OrderByCustomerIdSpec), customerId, TimeSpan.FromMinutes(10));
  }
}
