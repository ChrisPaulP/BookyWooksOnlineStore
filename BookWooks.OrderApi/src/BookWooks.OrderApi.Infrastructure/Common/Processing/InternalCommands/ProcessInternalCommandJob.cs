namespace BookWooks.OrderApi.Infrastructure.Common.Processing.InternalCommands;

[DisallowConcurrentExecution]
public class ProcessInternalCommandJob : IJob
{
  private readonly IMediator _mediator;
  public ProcessInternalCommandJob(IMediator mediator) => _mediator = mediator;

  public async Task Execute(IJobExecutionContext context)
  {
    await _mediator.Send(new ProcessInternalCommand());
  }
}
