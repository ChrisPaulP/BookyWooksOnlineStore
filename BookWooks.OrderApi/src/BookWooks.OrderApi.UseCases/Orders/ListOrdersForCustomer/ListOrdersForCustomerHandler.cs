using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.Core.OrderAggregate;
using BookWooks.OrderApi.Core.OrderAggregate.Specifications;
using BookWooks.OrderApi.UseCases.Orders.List;
using BookyWooks.SharedKernel;
using Microsoft.Extensions.Logging;

namespace BookWooks.OrderApi.UseCases.Orders.ListOrdersForCustomer;
public class ListOrdersForCustomerHandler : IQueryHandler<ListOrdersForCustomerQuery, Result<IEnumerable<OrderDTO>>>
{
  private readonly IReadRepository<Order> _readRepository;
  private readonly ILogger<ListOrdersForCustomerHandler> _logger;

  public ListOrdersForCustomerHandler(IReadRepository<Order> readRepository, ILogger<ListOrdersForCustomerHandler> logger)
  {
    _readRepository = readRepository;
    _logger = logger;
  }

  public async Task<Result<IEnumerable<OrderDTO>>> Handle(ListOrdersForCustomerQuery request, CancellationToken cancellationToken)
  {
    var spec = new OrderByCustomerIdSpec(request.CustomerId);
    //var order = await _repository.FindAsync(spec);
    var order = await _readRepository.FindAllAsync(spec);
    if (order == null)
    {
      return Result.NotFound();
    }
    var ordersByCustomerDtoList = order
        .Select(order => new OrderDTO(
            order.Id,
            order.Status.Name,
            order.OrderItems.Select(item => new OrderItemDTO(item.BookId, item.BookTitle, item.BookPrice, item.Quantity))//.ToList()
        ))
        .ToList();

    return ordersByCustomerDtoList;
  }
}
