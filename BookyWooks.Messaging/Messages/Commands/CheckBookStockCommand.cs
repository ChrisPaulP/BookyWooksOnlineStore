using BookyWooks.Messaging.Contracts.Events;

using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.Messaging.Contracts.Commands;

public record CheckBookStockCommand(Guid CorrelationId, Guid orderId, Guid customerId, decimal orderTotal, IEnumerable<OrderItemEventDto> orderItems) : MessageContract, CorrelatedBy<Guid>;

