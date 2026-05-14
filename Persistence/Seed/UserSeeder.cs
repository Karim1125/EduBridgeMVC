using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace EduBridgeMVC.Persistence.Seed;

public static class UserSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var adminEmail = DefaultUsers.AdminEmail;

        var existingUser = await userManager.FindByEmailAsync(adminEmail);

        if (existingUser is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Admin"
        };

        var result = await userManager.CreateAsync(admin, DefaultUsers.AdminPassword);

        if (!result.Succeeded)
            throw new Exception("Failed to create admin user");

        await userManager.AddToRoleAsync(admin, DefaultRoles.Admin);
    }
}