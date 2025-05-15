namespace BookyWooks.SharedKernel.Commands;
/// <summary>
/// Source: https://code-maze.com/cqrs-mediatr-fluentvalidation/
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
{
}
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
       where TCommand : ICommand
{
}