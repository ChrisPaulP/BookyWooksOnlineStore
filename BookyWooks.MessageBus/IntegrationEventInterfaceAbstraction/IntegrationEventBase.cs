using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.IntegrationEventInterfaceAbstraction;

public record IntegrationEventBase
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
}
