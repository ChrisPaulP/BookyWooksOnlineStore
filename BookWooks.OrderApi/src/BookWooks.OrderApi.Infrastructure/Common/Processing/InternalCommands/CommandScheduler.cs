


namespace BookWooks.OrderApi.Infrastructure.Common.Processing;
internal class CommandScheduler : ICommandScheduler
{
  private readonly ISqlConnectionFactory _sqlConnectionFactory;

  public CommandScheduler(ISqlConnectionFactory sqlConnectionFactory)
  {
    _sqlConnectionFactory = sqlConnectionFactory;
  }

  public async Task EnqueueAsync(ICommand command)
  {
    var connection = this._sqlConnectionFactory.GetOpenConnection();

    const string sqlInsert = "INSERT INTO [orders].[InternalCommand] ([Id], [EnqueueDate] , [Type], [Data]) VALUES " +
                             "(@Id, @EnqueueDate, @Type, @Data)";

    await connection.ExecuteAsync(sqlInsert, new
    {
      command.Id,
      EnqueueDate = DateTime.UtcNow,
      Type = command.GetType().FullName,
      Data = JsonConvert.SerializeObject(command, new JsonSerializerSettings
      {
        ContractResolver = new AllPropertiesContractResolver()
      })
    });
  }

  public async Task EnqueueAsync<T>(ICommand<T> command)
  {
    var connection = this._sqlConnectionFactory.GetOpenConnection();

    const string sqlInsert = "INSERT INTO [registrations].[InternalCommands] ([Id], [EnqueueDate] , [Type], [Data]) VALUES " +
                             "(@Id, @EnqueueDate, @Type, @Data)";

    await connection.ExecuteAsync(sqlInsert, new
    {
      command.Id,
      EnqueueDate = DateTime.UtcNow,
      Type = command.GetType().FullName,
      Data = JsonConvert.SerializeObject(command, new JsonSerializerSettings
      {
        ContractResolver = new AllPropertiesContractResolver()
      })
    });
  }
}
