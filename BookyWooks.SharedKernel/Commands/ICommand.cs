namespace BookyWooks.SharedKernel.Commands;
/// <summary>
/// Source: https://code-maze.com/cqrs-mediatr-fluentvalidation/
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
    Guid Id { get; }
}
public interface ICommand : IRequest
{
    Guid Id { get; }
}