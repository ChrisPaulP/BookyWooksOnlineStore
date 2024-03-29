using BookyWooks.SharedKernel;
using BookWooks.OrderApi.Core.OrderAggregate.Specifications;
using BookWooks.OrderApi.Core.OrderAggregate.Entities;


namespace BookWooks.OrderApi.UseCases.Orders.Get;
public class GetOrderDetailsHandler : IQueryHandler<GetOrderDetailsQuery, Result<OrderDTO>>
{
 // private readonly IRepository<Order> _repository;
  private readonly IReadRepository<Order> _readRepository;
  public GetOrderDetailsHandler(IReadRepository<Order> readRepository) //IRepository<Order> repository, 
  {
    //_repository = repository;
    _readRepository = readRepository;
  }

  public async Task<Result<OrderDTO>> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
  {
    var spec = new OrderByIdSpec(request.OrderId);
    //var order = await _repository.FindAsync(spec);
    var order = await _readRepository.FindAsync(spec);
    if (order == null)
    {
      return Result.NotFound();
    }
    // Project the orders into OrderDTO objects
    var orderDTO = new OrderDTO(order.Id, order.Status.Name, order.OrderItems.Select(item => new OrderItemDTO(item.ProductId, item.Price, item.Quantity)));//.ToList());

    return orderDTO;

  }
}
