using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace EduBridgeMVC.Persistence.Seed;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        var roles = new[]
        {
            DefaultRoles.Admin,
            DefaultRoles.Student,
            DefaultRoles.TA,
            DefaultRoles.Doctor
        };

        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role,
                    NormalizedName = role.ToUpper()
                });
    }
}