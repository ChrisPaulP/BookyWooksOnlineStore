using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

    //public const string StockConfirmedEventQueueName = "stock-confirmed-event-queue"; // remove this. It is just for test and needs to be removed

    //public const string THISISATEST = "THISISATEST";

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

        return queueName;
    }
}
