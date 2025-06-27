namespace BookyWooks.SharedKernel.Commands;

public abstract record CommandBase : ICommand
{
    public Guid Id { get; }
    protected CommandBase() => Id = Guid.NewGuid();
    protected CommandBase(Guid id) => Id = id;
}

public abstract record CommandBase<TResult> : ICommand<TResult>
{
    public Guid Id { get; }
    protected CommandBase() => Id = Guid.NewGuid();
    protected CommandBase(Guid id) => Id = id;

}
