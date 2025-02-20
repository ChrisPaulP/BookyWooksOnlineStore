namespace BookWooks.OrderApi.Infrastructure.Common.Processing.InternalCommands;
[DisallowConcurrentExecution]
public class ProcessInternalCommandJob : IJob
{
  public async Task Execute(IJobExecutionContext context)
  {
    await CommandsExecutor.Execute(new ProcessInternalCommand());
  }
}
