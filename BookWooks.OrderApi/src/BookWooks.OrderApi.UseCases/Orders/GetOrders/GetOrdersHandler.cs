﻿namespace BookWooks.OrderApi.UseCases.Orders.GetOrders;
public class GetOrdersHandler : IQueryHandler<GetOrdersQuery, DetailedResult<IEnumerable<OrderDTO>>>
{
    private readonly IReadRepository<Order> _repository;

    public GetOrdersHandler(IReadRepository<Order> repository)
  {
    _repository = repository;
  }

  public async Task<DetailedResult<IEnumerable<OrderDTO>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
  {
      var result = await _repository.ListAllAsync();
      var orders = result.Select(order => 
      new OrderDTO(order.Id,order.Status.Label,
      order.OrderItems?.Select(item => new OrderItemDTO(item.ProductId, item.Price, item.Quantity)) ?? Enumerable.Empty<OrderItemDTO>()
      //order.OrderItems.Select(item => new OrderItemDTO(item.BookId, item.BookTitle, item.BookPrice, item.Quantity))
      ));

      if (!orders.Any())
      {
        return StandardResult.NotFound("No orders were found");
      }
      foreach (var order in result)
      {
        Console.WriteLine($"Order Id: {order.Id}");
        Console.WriteLine($"Order Status: {order.Status.Label}");
        Console.WriteLine($"Order Items: {order.OrderItems}");
      }

    return StandardResult.Success(orders);
  }
}
