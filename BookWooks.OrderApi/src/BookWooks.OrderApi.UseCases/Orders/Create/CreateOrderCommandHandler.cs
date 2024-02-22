
using Ardalis.GuardClauses;



using BookWooks.OrderApi.Core.OrderAggregate;

using BookWooks.OrderApi.UseCases.Create;

using BookWooks.OrderApi.UseCases.Orders;
using BookyWooks.SharedKernel;
using Microsoft.Extensions.Logging;

namespace BookWooks.OrderApi.UseCases.Contributors.Create;
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Result<Guid>>
{
  private readonly ILogger<CreateOrderCommandHandler> _logger;
  private readonly IRepository<Order> _orderRepository;

  public CreateOrderCommandHandler(ILogger<CreateOrderCommandHandler> logger, IRepository<Order> orderRepository)
  {
    _logger = logger;
    _orderRepository = orderRepository;
  }

  public async Task<Result<Guid>> Handle(CreateOrderCommand request,
    CancellationToken cancellationToken)
  {
    var deliveryAddress = new DeliveryAddress(request.Street, request.City, request.Country, request.Postcode);
    var userId = (!string.IsNullOrEmpty(request.UserId)) ? request.UserId : "1";
    var userName = (!string.IsNullOrEmpty(request.UserName)) ? request.UserName : "Christopher Porter";
    var newOrder = new Order(userId, userName, deliveryAddress, request.CardNumber, request.CardSecurityNumber, request.CardHolderName);
    if (newOrder == null)
      throw new NotFoundException("", "");
    foreach (var item in request.OrderItems)
    {
      newOrder.AddOrderItem(item.BookId, item.BookTitle, item.BookPrice, item.BookImageUrl, item.Quantity);
    }
    _logger.LogInformation("----- Creating Order - Order: {@Order}", newOrder);
    
    await _orderRepository.AddAsync(newOrder);

    await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    return Result.Success(newOrder.Id);
    //return newOrder.Id;
  }
}

