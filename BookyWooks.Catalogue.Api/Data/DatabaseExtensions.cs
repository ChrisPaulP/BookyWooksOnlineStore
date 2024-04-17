using Marten.Schema;
using Microsoft.EntityFrameworkCore;

namespace BookyWooks.Catalogue.Api.Data;

public static class DatabaseExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<CatalogueDbContext>();

        context.Database.MigrateAsync().GetAwaiter().GetResult();

        await SeedAsync(context);
    }

    private static async Task SeedAsync(CatalogueDbContext context)
    {      
        await SeedProductAsync(context);
    }
    private static async Task SeedProductAsync(CatalogueDbContext context)
    {
        if (!await context.Products.AnyAsync())
        {
            await context.Products.AddRangeAsync(InitialData.Products);
            await context.SaveChangesAsync();
        }
    }
}
