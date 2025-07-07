using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
using BookyWooks.SharedKernel.InternalCommands;
using MediatR;
using Newtonsoft.Json;

namespace BookWooks.OrderApi.UseCases.InternalCommands;
public class CompletePaymentInternalCommand : InternalCommandBase
{
  [JsonConstructor]
  public CompletePaymentInternalCommand(Guid id, Guid customerId) : base(id) => CustomerId = customerId;
  public Guid CustomerId { get; }
}

public class CompletePaymentInternalCommandHandler : IRequestHandler<CompletePaymentInternalCommand>
{
  private readonly IRepository<Order> _repository;

  public CompletePaymentInternalCommandHandler(IRepository<Order> repository) => _repository = repository;

    public async Task Handle(CompletePaymentInternalCommand command, CancellationToken cancellationToken)
    {
        // Fix for CS1579: Await the Task<List<Order>> to get the actual list of orders
        var orders = await _repository.FindAllAsync(new OrderByCustomerIdSpec(CustomerId.From(command.CustomerId)));

    foreach (var order in orders)
    {
      try
      {
        var updatedOrder = order.ApproveOrder();
        await _repository.UpdateAsync(updatedOrder);
        //await _repository.UpdateAsync(order);

      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        throw; 
      }
    }
    await _repository.UnitOfWork.SaveEntitiesAsync();

  }
}

