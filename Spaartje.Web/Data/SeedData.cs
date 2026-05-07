// WEB/Data/SeedData.cs
using Microsoft.AspNetCore.Identity;
using WEB.Models;

namespace WEB.Data;

public static class SeedData
{
    // WHY static async Task?
    // Seeding runs once at startup, before any HTTP requests are served.
    // We need async because Identity's methods (CreateAsync, etc.) are
    // all async — using .Result or .Wait() here would risk deadlocks.
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // --- Seed Roles ---
        // WHY check RoleExistsAsync first?
        // Seeding runs every startup. Without the check, you'd get
        // duplicate key exceptions after the first run.
        string[] roles = ["Admin", "User"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // --- Seed Admin User ---
        const string adminEmail = "admin@financialdashboard.com";
        const string adminPassword = "Admin@123!"; // Use env variable in production!

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true, // Skip email confirmation for seed user
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (!result.Succeeded)
            {
                // Surface errors clearly during development
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create admin user: {errors}");
            }
        }

        // Assign Admin role (also safe to call multiple times)
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}