using BookyWooks.SharedKernel.Commands;

namespace BookyWooks.SharedKernel.InternalCommands;

public abstract class InternalCommandBase : ICommand
{
    public Guid Id { get; }
    protected InternalCommandBase() => Id = Guid.NewGuid();
    protected InternalCommandBase(Guid id) => Id = id;
}

public abstract class InternalCommandBase<TResult> : ICommand<TResult>
{
    public Guid Id { get; }
    protected InternalCommandBase() => Id = Guid.NewGuid();
    protected InternalCommandBase(Guid id) => Id = id; 
}
