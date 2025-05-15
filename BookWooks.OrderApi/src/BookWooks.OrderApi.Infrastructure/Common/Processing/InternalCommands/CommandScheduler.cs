


namespace BookWooks.OrderApi.Infrastructure.Common.Processing;
internal class CommandScheduler : ICommandScheduler
{
  private readonly BookyWooksOrderDbContext _dbContext;

  public CommandScheduler(BookyWooksOrderDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task EnqueueAsync(ICommand command)
  {
    var internalCommand = new InternalCommand
    {
      Id = command.Id,
      EnqueueDate = DateTime.UtcNow,
      Type = command.GetType().FullName ?? string.Empty, 
      Data = JsonConvert.SerializeObject(command, new JsonSerializerSettings
      {
        ContractResolver = new AllPropertiesContractResolver()
      })
    };

    await _dbContext.InternalCommands.AddAsync(internalCommand);
    await _dbContext.SaveChangesAsync();
  }

  public async Task EnqueueAsync<T>(ICommand<T> command)
  {
    var internalCommand = new InternalCommand
    {
      Id = command.Id,
      EnqueueDate = DateTime.UtcNow,
      Type = command.GetType().FullName ?? string.Empty, 
      Data = JsonConvert.SerializeObject(command, new JsonSerializerSettings
      {
        ContractResolver = new AllPropertiesContractResolver()
      })
    };

    await _dbContext.InternalCommands.AddAsync(internalCommand);
    await _dbContext.SaveChangesAsync();
  }
}

