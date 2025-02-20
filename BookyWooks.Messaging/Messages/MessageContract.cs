using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.Messaging.Contracts;

public abstract record MessageContract
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string MessageType { get; init; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public DateTime? ProcessedDate { get; init; }

    protected MessageContract(string messageType)
    {
        MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType), "MessageType cannot be null.");
    }
}
