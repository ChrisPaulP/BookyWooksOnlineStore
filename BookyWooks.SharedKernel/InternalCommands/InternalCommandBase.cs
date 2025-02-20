using BookyWooks.SharedKernel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.SharedKernel.InternalCommands;

public abstract class InternalCommandBase : ICommand
{
    protected InternalCommandBase(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}

public abstract class InternalCommandBase<TResult> : ICommand<TResult>
{
    protected InternalCommandBase()
    {
        Id = Guid.NewGuid();
    }

    protected InternalCommandBase(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}
