using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace EduBridgeMVC.Persistence.Seed;

public static class UserRoleSeeder
{
    public static async Task SeedUserRolesAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        var adminEmail = DefaultUsers.AdminEmail;

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
            return;

        var roleExists = await roleManager.RoleExistsAsync(DefaultRoles.Admin);

        if (!roleExists)
            return;

        var isInRole = await userManager.IsInRoleAsync(admin, DefaultRoles.Admin);

        if (!isInRole) await userManager.AddToRoleAsync(admin, DefaultRoles.Admin);
    }
}