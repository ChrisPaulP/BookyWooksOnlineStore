using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.SharedKernel.DomainEventMapper;

public interface IDomainEventMapper
{
    string GetName(Type type);

    Type GetType(string name);
}
