namespace BookWooks.OrderApi.Infrastructure.Common.Processing.InternalCommands;
internal class ProcessInternalCommandHandler : ICommandHandler<ProcessInternalCommand>
{
  private readonly BookyWooksOrderDbContext _dbContext;
  private readonly IMediator _mediator;

  public ProcessInternalCommandHandler(BookyWooksOrderDbContext dbContext, IMediator mediator)
  {
    _dbContext = dbContext;
    _mediator = mediator;
  }

  public async Task Handle(ProcessInternalCommand command, CancellationToken cancellationToken)
  {
    var commands = await _dbContext.InternalCommands
        .Where(c => c.ProcessedDate == null)
        .OrderBy(c => c.EnqueueDate)
        .ToListAsync(cancellationToken);
    
    if (commands.Count == 0)
      return;

    var policy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
        [
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
        ]);

    foreach (var internalCommand in commands)
    {
      var result = await policy.ExecuteAndCaptureAsync(() => ProcessCommand(internalCommand));

      if (result.Outcome == OutcomeType.Failure)
      {
        internalCommand.ProcessedDate = DateTime.UtcNow;
        internalCommand.Error = result.FinalException.ToString();
        _dbContext.InternalCommands.Update(internalCommand);
      }
      else if (result.Outcome == OutcomeType.Successful)
      {
        internalCommand.ProcessedDate = DateTime.UtcNow;
        _dbContext.InternalCommands.Update(internalCommand);
      }
    }
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  private async Task ProcessCommand(InternalCommand internalCommand)
  {
    Type? type = Type.GetType(internalCommand.Type) ?? throw new InvalidOperationException($"Type '{internalCommand.Type}' could not be found.");

    var commandToProcess = JsonConvert.DeserializeObject(internalCommand.Data, type)
                               ?? throw new InvalidOperationException($"Deserialization of command data failed for type '{internalCommand.Type}'.");

    await _mediator.Send(commandToProcess);
  }
}

