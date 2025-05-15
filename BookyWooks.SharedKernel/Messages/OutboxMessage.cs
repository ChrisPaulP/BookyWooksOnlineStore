using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.SharedKernel.Messages;

public record OutboxMessage(Guid Id, string MessageType, string Message, DateTime OccurredOn, DateTime? ProcessedDate = null);
