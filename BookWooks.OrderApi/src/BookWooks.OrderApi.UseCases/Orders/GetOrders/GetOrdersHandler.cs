using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookWooks.OrderApi.UseCases.Contributors;
using BookyWooks.SharedKernel;

namespace BookWooks.OrderApi.UseCases.Orders.GetOrders;
public class GetOrdersHandler : IQueryHandler<GetOrdersQuery, Result<IEnumerable<OrderDTO>>>
{
    private readonly IReadRepository<Order> _repository;

    public GetOrdersHandler(IReadRepository<Order> repository)
  {
    _repository = repository;
  }

  public async Task<Result<IEnumerable<OrderDTO>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
  {
      var result = await _repository.ListAllAsync();
      var orders = result.Select(order => 
      new OrderDTO(order.Id,order.Status.Name,
      null
      //order.OrderItems.Select(item => new OrderItemDTO(item.BookId, item.BookTitle, item.BookPrice, item.Quantity))
      ));

      foreach (var order in result)
      {
        Console.WriteLine($"Order Id: {order.Id}");
        Console.WriteLine($"Order Status: {order.Status.Name}");
        Console.WriteLine($"Order Items: {order.OrderItems}");
      }

    return Result.Success(orders);
  }
}
