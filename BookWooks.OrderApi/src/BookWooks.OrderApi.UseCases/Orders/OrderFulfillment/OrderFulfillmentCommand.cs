using BookyWooks.SharedKernel.Commands;
using BookyWooks.SharedKernel.ResultPattern;

namespace BookWooks.OrderApi.UseCases.Orders.OrderFulfillment;
public record OrderFulfillmentCommand(Guid OrderId) : ICommand<DetailedResult<Guid>>;

