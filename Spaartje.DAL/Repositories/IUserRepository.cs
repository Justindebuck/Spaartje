using Spaartje.Domain.Models;

namespace Spaartje.DAL.Repositories;

// An interface is a CONTRACT. It says:
// "Any class that implements IUserRepository MUST have these methods."
// The BLL will depend on this interface, not on the concrete class.
// This means the BLL doesn't care HOW users are fetched — just that they can be.
public interface IUserRepository
{
    // Get all users from the database.
    // Returns a list of our Domain User model (not IdentityUser).
    Task<List<User>> GetAllUsersAsync();

    // Find a single user by their email address.
    // Returns null if no user with that email exists.
    Task<User?> GetUserByEmailAsync(string email);

    Task<User?> GetUserByIdAsync(int id);

    Task AddUserAsync(User user);

    Task<bool> EmailExistsAsync(string email);

    Task DeleteUserAsync(int userId);
}