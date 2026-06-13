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
    
    }

    // GetUserByEmailAsync finds a single user by email.
    public async Task<User?> GetUserByEmailAsync(string email)
    {
     
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
     
    }

    public async Task AddUserAsync(User user)
    {
     
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
     
    }

}