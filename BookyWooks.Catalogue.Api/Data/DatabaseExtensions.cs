using Marten.Schema;
using Microsoft.EntityFrameworkCore;

namespace BookyWooks.Catalogue.Api.Data;

public static class DatabaseExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<CatalogueDbContext>();

        await context.Database.MigrateAsync();
        await ClearData(context);
        await SeedAsync(context);
    }

    private static async Task ClearData(CatalogueDbContext context)
    {
        context.Products.RemoveRange(context.Products);
        await context.SaveChangesAsync();
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
