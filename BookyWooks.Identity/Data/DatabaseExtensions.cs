using BookyWooks.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BookyWooks.Identity.Data;

public static class DatabaseExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IdentityDbContext>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();
        await ClearData(context);
        await SeedAsync(context, userManager, roleManager, logger);
    }

    private static async Task ClearData(IdentityDbContext context)
    {
        context.Users.RemoveRange(context.Users);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAsync(IdentityDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<IdentityDbContext> logger)
    {

        string password = GeneratePassword(length: 12, numberOfNonAlphanumericCharacters: 2);
        var existingRoles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
        var rolesToAdd = InitialData.Roles
            .Where(role => !existingRoles.Contains(role.Name))
            .ToList();

        var existingUsers = await userManager.Users.Select(u => u.UserName).ToListAsync();
        var usersToAdd = InitialData.Users
            .Where(user => !existingUsers.Contains(user.UserName))
            .ToList();

        foreach (var role in rolesToAdd)
        {
            await roleManager.CreateAsync(role);
        }

        foreach (var user in usersToAdd)
        {
            var creationResult = await userManager.CreateAsync(user, "Password1!"); // replace "Password1!" with the actual password
            if (creationResult.Succeeded)
            {
                var userRoles = InitialData.UserRoles
                    .Where(ur => ur.UserName == user.UserName)
                    .Select(ur => ur.RoleName)
                    .ToArray();

                if (userRoles.Any())
                {
                    await userManager.AddToRolesAsync(user, userRoles);
                }
            }
            else
            {
                    logger.LogError($"Error creating user: {user.UserName}");
            }
        }
    }

    private static string GeneratePassword(int length, int numberOfNonAlphanumericCharacters)
    {
        const string alphanumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        const string nonAlphanumericChars = "!@#$%^&*()";

        var random = new Random();
        var password = new StringBuilder();

        // Generate alphanumeric characters
        for (int i = 0; i < length - numberOfNonAlphanumericCharacters; i++)
        {
            password.Append(alphanumericChars[random.Next(alphanumericChars.Length)]);
        }

        // Generate non-alphanumeric characters
        for (int i = 0; i < numberOfNonAlphanumericCharacters; i++)
        {
            password.Append(nonAlphanumericChars[random.Next(nonAlphanumericChars.Length)]);
        }

        return password.ToString();
    }
}
