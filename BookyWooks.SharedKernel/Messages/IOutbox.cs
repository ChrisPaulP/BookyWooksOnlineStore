using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.SharedKernel.Messages;

public interface IOutbox
{
    void Add(OutboxMessage message);

    Task Save();
}
