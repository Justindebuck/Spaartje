using Microsoft.AspNetCore.Identity;
using Spaartje.Domain.Models;

namespace Spaartje.DAL.Repositories;

// UserRepository implements IUserRepository.
// This is the class that actually runs the database queries.
// It uses UserManager<IdentityUser> from ASP.NET Identity to access the database.
//
// Why use UserManager instead of DbContext directly?
// UserManager already handles all the Identity-specific logic (password hashing,
// role lookups, etc.). It's safer and simpler than writing raw EF Core queries
// for user data.
public class UserRepository : IUserRepository
{
    // UserManager gives us access to the AspNetUsers table and related tables.
    private readonly UserManager<IdentityUser> _userManager;

    // Constructor injection — ASP.NET provides UserManager automatically.
    public UserRepository(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    // GetAllUsersAsync fetches every user from the database
    // and maps them to our Domain User model.
    public async Task<List<User>> GetAllUsersAsync()
    {
        // _userManager.Users queries the AspNetUsers table.
        // .ToList() executes the query and loads all users into memory.
        var identityUsers = _userManager.Users.ToList();

        // We create an empty list to hold our Domain User objects.
        var users = new List<User>();

        // For each IdentityUser, we:
        // 1. Get their roles from the AspNetUserRoles table
        // 2. Map the IdentityUser to our simpler Domain User model
        foreach (var identityUser in identityUsers)
        {
            // GetRolesAsync queries AspNetUserRoles + AspNetRoles
            // to get the role names for this specific user.
            var roles = await _userManager.GetRolesAsync(identityUser);

            // Create a Domain User object from the IdentityUser data.
            // This is called "mapping" — translating between two different
            // representations of the same data.
            users.Add(new User
            {
                Id = identityUser.Id,
                Email = identityUser.Email ?? string.Empty,
                EmailConfirmed = identityUser.EmailConfirmed,
                Roles = roles.ToList()
            });
        }

        return users;
    }

    // GetUserByEmailAsync finds a single user by email.
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        // FindByEmailAsync queries AspNetUsers WHERE NormalizedEmail = email.
        // Returns null if not found.
        var identityUser = await _userManager.FindByEmailAsync(email);

        // If no user found, return null.
        if (identityUser == null)
            return null;

        var roles = await _userManager.GetRolesAsync(identityUser);

        // Map and return the Domain User.
        return new User
        {
            Id = identityUser.Id,
            Email = identityUser.Email ?? string.Empty,
            EmailConfirmed = identityUser.EmailConfirmed,
            Roles = roles.ToList()
        };
    }
}