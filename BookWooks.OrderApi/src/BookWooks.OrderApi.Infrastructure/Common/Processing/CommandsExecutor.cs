namespace BookWooks.OrderApi.Infrastructure.Common.Processing;
internal static class CommandsExecutor
{
  internal static async Task Execute(ICommand command)
  {
    using (var scope = OrderCompositionRoot.BeginLifetimeScope())
    {
      var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
      await mediator.Send(command);
    }
  }

  internal static async Task<TResult> Execute<TResult>(ICommand<TResult> command)
  {
    using (var scope = OrderCompositionRoot.BeginLifetimeScope())
    {
      var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
      return await mediator.Send(command);
    }
  }
}

