namespace BookWooks.OrderApi.Core.Orders;
public interface IOrderIntegrationEventService
{Task PublishEventsThroughEventBusAsync(Guid transactionId);
  Task AddAndSaveEventAsync(IntegrationEventBase evt);
}
