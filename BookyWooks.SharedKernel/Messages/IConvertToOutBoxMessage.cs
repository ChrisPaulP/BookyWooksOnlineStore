namespace BookyWooks.SharedKernel.Messages;

public interface IConvert
{
    OutboxMessage ToOutboxMessage();
}
