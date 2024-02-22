using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.OrderApi.Core.OrderAggregate.Specifications;
public class OrderByCustomerIdSpec : BaseSpecification<Order>
{
  public OrderByCustomerIdSpec(int customerId)
       : base(x => x.CustomerId == customerId)
  {
    AddInclude(x => x.OrderItems);
    EnableCache(nameof(OrderByCustomerIdSpec), customerId, TimeSpan.FromMinutes(10));
  }
}
