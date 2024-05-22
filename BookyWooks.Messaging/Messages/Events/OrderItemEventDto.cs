using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.Messaging.Contracts.Events;

public record OrderItemEventDto(Guid ProductId, int quantity, bool hasStock = false);
