namespace BookWooks.OrderApi.UseCases.Orders.OrderFulfillment;
public record OrderFulfillmentCommand(Guid OrderId) : ICommand<SetOrderStatusResult>
{
    public Guid Id => OrderId;
}

