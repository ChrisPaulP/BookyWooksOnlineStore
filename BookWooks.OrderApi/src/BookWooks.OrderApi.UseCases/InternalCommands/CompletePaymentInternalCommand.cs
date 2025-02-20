using BookyWooks.SharedKernel.InternalCommands;
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

