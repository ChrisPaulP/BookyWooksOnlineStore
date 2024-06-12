using BookyWooks.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace BookyWooks.Identity.Data;

public static class InitialData
{
    public static List<IdentityRole> Roles { get; } = new List<IdentityRole>
    {
        new IdentityRole("Administrator"),
        new IdentityRole("BookManager"),
        // Add other roles as needed
    };

    public static List<ApplicationUser> Users { get; } = new List<ApplicationUser>
    {
        new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" },
        new ApplicationUser { UserName = "bookmanager@localhost", Email = "bookmanager@localhost" },
        // Add other users as needed
    };

    public static List<(string UserName, string RoleName)> UserRoles { get; } = new List<(string UserName, string RoleName)>
    {
        ("administrator@localhost", "Administrator"),
        ("bookmanager@localhost", "BookManager"),
        // Add other user-role mappings as needed
    };
}
