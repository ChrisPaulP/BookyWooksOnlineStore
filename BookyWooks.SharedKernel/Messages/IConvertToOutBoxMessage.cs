namespace BookyWooks.SharedKernel.Messages;

public interface IConvertToOutBoxMessage
{
    OutboxMessage ToOutboxMessage();
}
