using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.Messaging.Events;

public record StockConfirmedEvent(Guid orderId, IEnumerable<OrderItemEventDto> stockItems) : IntegrationEvent;
