namespace BookWooks.OrderApi.Infrastructure.Common.Processing.Outbox;

[DisallowConcurrentExecution]
public class ProcessOutboxJob : IJob
{
  public async Task Execute(IJobExecutionContext context)
  {
    await CommandsExecutor.Execute(new ProcessOutboxCommand());
  }
}
