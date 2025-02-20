using System.Text.RegularExpressions;


namespace BookyWooks.Messaging.Constants;

public class QueueConstants
{
    // events
    //public const string OrderCreatedEventQueueName = "order-created-queue";
    //public const string OrderCompletedEventQueueName = "order-completed-queue";
    //public const string OrderFailedEventQueueName = "order-failed-queue";
    //public const string UserCreatedEventQueueName = "user-created-event-queue";

    // messages
    public const string CreateOrderMessageQueueName = "create-order-message-queue";
    public const string CompletePaymentCommandQueueName = "complete-payment-command-queue";
    //public const string StockRollBackMessageQueueName = "stock-rollback-message-queue";
    public const string CheckBookStockCommandQueueName = "check-book-stock-command-queue";
    public static string GetQueueNameForConsumer(Type consumerType)
    {
        // Remove "Consumer" suffix from the consumer type name
        var consumerName = consumerType.Name.Replace("Consumer", "");

        // Match camel case or Pascal case naming conventions
        var matches = Regex.Matches(consumerName, @"([A-Z]?[a-z]+)");

        // Join the matched words with dashes to create the queue name
        var queueName = string.Join("-", matches.Select(m => m.Value.ToLower()));

        // Append "_queue" to the queue name
        queueName += "-queue";

        return queueName switch
        {
            var name when ValidQueueNames.Contains(name) => name,
            _ => consumerType.Name
        };
    }
    private static readonly HashSet<string> ValidQueueNames = new()
    {
        CreateOrderMessageQueueName,
        CompletePaymentCommandQueueName,
        CheckBookStockCommandQueueName
    };
}
