using BookyWooks.Messaging.Contracts.Commands;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.Messaging.Contracts.Commands;

public record CompletePaymentCommand(Guid CorrelationId, Guid customerId, decimal orderTotal) : MessageContract, CorrelatedBy<Guid>;

