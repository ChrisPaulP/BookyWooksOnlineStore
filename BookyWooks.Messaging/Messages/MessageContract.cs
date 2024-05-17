using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.Messaging.Contracts;

public abstract record MessageContract
{
    DateTime OccurredOn { get; init; } = DateTime.Now;
}
