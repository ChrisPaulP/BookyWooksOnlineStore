using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq.Expressions;

namespace IntegrationTestingSetup;

public class ApiTestBase<TProgram, TDbContext> : IAsyncLifetime
        where TProgram : class // Ensure the TProgram is a class (e.g. Startup or Program)
        where TDbContext : DbContext // Ensure TDbContext is a type of DbContext
{
    private readonly Func<Task> _resetDatabase;
    private readonly TestFactoryBase<TProgram> _apiFactory;
    private static IServiceScopeFactory _scopeFactory = null!;
    //private static TDbContext _dbContext = null!;

    public ApiTestBase(TestFactoryBase<TProgram> apiFactory, Func<Task> resetDatabase)
    {
        _apiFactory = apiFactory;
        _scopeFactory = _apiFactory.Services.GetRequiredService<IServiceScopeFactory>();
        _resetDatabase = resetDatabase; // Pass in the resetDatabase action as a dependency
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public static async Task AddAsync<TEntity>(TEntity entity)
       where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync();
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
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        return await dbContext.FindAsync<TEntity>(keyValues);
    }
    public static async Task<IEnumerable<TEntity?>> FindAllAsync<TEntity>()
     where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        return await dbContext.Set<TEntity>().ToListAsync();
    }
    public static async Task<List<TEntity>> FindByForeignKeyAsync<TEntity>(
    Expression<Func<TEntity, bool>> predicate)
    where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        return await dbContext.Set<TEntity>().Where(predicate).ToListAsync();
    }
    public Task DisposeAsync() => _resetDatabase();
}

