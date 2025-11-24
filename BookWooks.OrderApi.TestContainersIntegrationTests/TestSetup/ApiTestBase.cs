namespace BookWooks.OrderApi.TestContainersIntegrationTests.TestSetup;

public class ApiTestBase<TProgram, TDbContext>
    where TProgram : class
    where TDbContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ApiTestBase(WebApplicationFactory<TProgram> apiFactory)
    {
        _scopeFactory = apiFactory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    protected async Task AddAsync<TEntity>(TEntity entity) where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        db.Add(entity);
        await db.SaveChangesAsync();
    }

    protected async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        return await mediator.Send(request);
    }

    protected async Task<TEntity?> FindAsync<TEntity>(params object[] keys) where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await db.FindAsync<TEntity>(keys);
    }

    protected async Task<List<TEntity>> FindByForeignKeyAsync<TEntity>(
        Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await db.Set<TEntity>().Where(predicate).ToListAsync();
    }
}

