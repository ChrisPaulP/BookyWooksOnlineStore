using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.Messaging.Contracts.Events;

public record OrderFailedEvent(Guid orderId, Guid customerId, string ErrorMessage);

