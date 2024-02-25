namespace BookWooks.OrderApi.TestContainersIntegrationTests;

//We create a new instance of the class for every test, which means we create and destroy the class once per test. (This is irrespective of whether you use IAsyncLifetime.)
//If you want to do initialization and cleanup work only one time for a class, you can do that with a Class Fixture.See https://xunit.net/docs/shared-context for more information.

[Collection("Order Test Collection")]
public class OrderApiBaseIntegrationTest : IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;
    private readonly OrderApiApplicationFactory<Program> _apiFactory;
    private static IServiceScopeFactory _scopeFactory = null!;
    private static BookyWooksOrderDbContext _bookyWooksOrderDbContext = null!;

    public OrderApiBaseIntegrationTest(OrderApiApplicationFactory<Program> apiFactory)
    {
        _apiFactory = apiFactory;
        _scopeFactory = _apiFactory.Services.GetRequiredService<IServiceScopeFactory>();
        //_resetDatabase = apiFactory.ResetDatabaseAsync;
        _resetDatabase = apiFactory.DisposeAsync;
    }
    public Task InitializeAsync() => Task.CompletedTask;

    public static async Task AddAsync<TEntity>(TEntity entity)
       where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        _bookyWooksOrderDbContext = scope.ServiceProvider.GetRequiredService<BookyWooksOrderDbContext>();

        _bookyWooksOrderDbContext.Add(entity);

        await _bookyWooksOrderDbContext.SaveChangesAsync();
    }
    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        return await mediator.Send(request);
    }
    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
       where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        _bookyWooksOrderDbContext = scope.ServiceProvider.GetRequiredService<BookyWooksOrderDbContext>();

        return await _bookyWooksOrderDbContext.FindAsync<TEntity>(keyValues);
    }
    public Task DisposeAsync() => _resetDatabase();
}
