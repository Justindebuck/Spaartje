using Spaartje.Domain.Models;

namespace Spaartje.BLL.Services;

// IUserService defines what user-related operations the application supports.
// The Web layer depends on this interface.
// This means Web never knows HOW users are fetched — it just asks the service.
//
// In the future, if you add rules like "only show verified users",
// you add that rule HERE in the service, not in the Web layer.
// The Web layer code doesn't change at all.
public interface IUserService
{
    // Get all users. Returns Domain User objects (not IdentityUser).
    Task<List<User>> GetAllUsersAsync();

    // Get a single user by email. Returns null if not found.
    Task<User?> GetUserByEmailAsync(string email);
}