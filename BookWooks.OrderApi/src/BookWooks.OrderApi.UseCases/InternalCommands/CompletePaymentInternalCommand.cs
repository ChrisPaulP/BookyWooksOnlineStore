using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
using BookyWooks.SharedKernel.InternalCommands;
using MediatR;
using Newtonsoft.Json;

namespace BookWooks.OrderApi.UseCases.InternalCommands;
public class CompletePaymentInternalCommand : InternalCommandBase
{
  [JsonConstructor]
  public CompletePaymentInternalCommand(Guid id, Guid customerId) : base(id)
  {
    CustomerId = customerId;

  }
  public Guid CustomerId { get; }
}


public class CompletePaymentInternalCommandHandler : IRequestHandler<CompletePaymentInternalCommand>
{
  private readonly IRepository<Order> _repository;

  public CompletePaymentInternalCommandHandler(IRepository<Order> repository) => _repository = repository;

    public async Task Handle(CompletePaymentInternalCommand command, CancellationToken cancellationToken)
    {
        // Fix for CS1579: Await the Task<List<Order>> to get the actual list of orders
        var orders = await _repository.FindAllAsync(new OrderByIdSpec(OrderId.From(command.Id)));

        foreach (var order in orders)
        {
            // TODO: Update order payment status as needed
            // order.MarkPaymentComplete();
        }

        // Fix for CS0103: Replace '_dbContext' with '_repository' or the correct context object
        //await _repository.AddAsync(cancellationToken);
    }
}

