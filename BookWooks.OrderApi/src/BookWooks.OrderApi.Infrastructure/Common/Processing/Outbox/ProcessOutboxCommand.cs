namespace BookWooks.OrderApi.Infrastructure.Common.Processing.Outbox;
public record ProcessOutboxCommand : CommandBase, IRecurringCommand;
