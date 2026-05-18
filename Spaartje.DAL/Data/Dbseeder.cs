using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Spaartje.DAL.Data;

public static class DbSeeder
{
    public const string AdminRole = "Admin";

    private const string AdminEmail = "admin@spaartje.nl";
    private const string AdminPassword = "Admin123!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var roleExists = await roleManager.RoleExistsAsync(AdminRole);

        if (!roleExists)
        {
            var role = new IdentityRole(AdminRole);

            await roleManager.CreateAsync(role);
        }

        var adminUser = await userManager.FindByEmailAsync(AdminEmail);

        if (adminUser == null)
        {
            var newAdmin = new IdentityUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(newAdmin, AdminPassword);

            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, AdminRole);
            }
            else
            {
                foreach (var error in createResult.Errors)
                {
                    Console.WriteLine($"[Seeder Error] {error.Description}");
                }
            }
        }
        else
        {
            var isInRole = await userManager.IsInRoleAsync(adminUser, AdminRole);
            if (!isInRole)
            {
                await userManager.AddToRoleAsync(adminUser, AdminRole);
            }
        }
    }
}