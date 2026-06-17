using BCrypt.Net;
using Microsoft.Extensions.DependencyInjection;
using Spaartje.DAL.Repositories;
using Spaartje.Domain.Models;


namespace Spaartje.DAL.Data;

public static class DbSeeder
{
    public const string AdminRole = "Admin";

    private const string AdminEmail = "admin@spaartje.nl";
    private const string AdminPassword = "Admin123!";
    private const string AdminUserName = "Admin";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();

        // Check if the admin user already exists
        var existingAdmin = await userRepository.GetUserByEmailAsync(AdminEmail);

        if (existingAdmin == null)
        {
            var adminUser = new User
            {
                Email     = AdminEmail,
                UserName  = AdminUserName,
                Password  = BCrypt.Net.BCrypt.HashPassword(AdminPassword), 
                Role      = AdminRole,
                CreatedAt = DateTime.UtcNow
            };

            await userRepository.AddUserAsync(adminUser);

            Console.WriteLine("[Seeder] Admin user created.");
        }
        else
        {
            Console.WriteLine("[Seeder] Admin user already exists, skipping.");
        }
    }
}