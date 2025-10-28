using BookWooks.OrderApi.Core.OrderAggregate.DomainValidation;
using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookWooks.OrderApi.Core.OrderAggregate.Specifications;
using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;

using BookyWooks.SharedKernel.InternalCommands;
using BookyWooks.SharedKernel.Repositories;
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
        var orders = await _repository.FindAllAsync(new OrderByCustomerIdSpec(CustomerId.From(command.CustomerId)));

      foreach (var order in orders)
      {
      var approvalResult = order.ApproveOrder().Match(
        order => _repository.UpdateAsync(order),
        error => throw new InvalidOperationException($"Failed to approve order {order.OrderId}: {error}"));
    }
    
      await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
 }
}

