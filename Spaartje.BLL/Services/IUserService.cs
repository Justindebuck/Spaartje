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
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByEmailAsync(string email);

    Task<User?> GetUserByIdAsync(int id);

    Task<(bool Success, string Error)> RegisterAsync(string email, string userName, string password);
    Task<User?> ValidateLoginAsync(string email, string password);

    Task<string?> DeleteUserAsync(int userIdToDelete, int requestingUserId);
}