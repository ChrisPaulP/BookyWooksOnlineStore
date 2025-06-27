namespace BookWooks.OrderApi.Infrastructure.Common.Processing.Outbox;
[DisallowConcurrentExecution]
public class ProcessOutboxJob : IJob
{
  private readonly IMediator _mediator;
  public ProcessOutboxJob(IMediator mediator) => _mediator = mediator;

  public async Task Execute(IJobExecutionContext context)
  {
    await _mediator.Send(new ProcessOutboxCommand());
  }
}
