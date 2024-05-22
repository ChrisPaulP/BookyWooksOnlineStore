using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.Messaging.Contracts.Events;

public record StockReservationFailedEvent(Guid CorrelationId, Guid orderId, string ErrorMessage, IEnumerable<OrderItemEventDto> stockItems) : MessageContract, CorrelatedBy<Guid>;
