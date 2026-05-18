using Spaartje.DAL.Repositories;
using Spaartje.Domain.Models;

namespace Spaartje.BLL.Services;

// UserService implements IUserService.
// It is the bridge between the Web layer and the DAL.
//
// Currently it mostly passes calls through to the repository.
// This might seem pointless now, but as the app grows,
// business rules get added HERE before the data goes back to Web.
//
// Example future rule:
//   public async Task<List<User>> GetAllUsersAsync()
//   {
//       var users = await _userRepository.GetAllUsersAsync();
//       return users.Where(u => u.EmailConfirmed).ToList(); // ← business rule
//   }
public class UserService : IUserService
{
    // The service depends on the INTERFACE (IUserRepository), not the concrete class.
    // This means we could swap the database engine without changing this service.
    private readonly IUserRepository _userRepository;

    // ASP.NET injects IUserRepository automatically via dependency injection.
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // Get all users — delegates to the repository.
    public async Task<List<User>> GetAllUsersAsync()
    {
        // In the future, add business rules here before returning.
        return await _userRepository.GetAllUsersAsync();
    }

    // Get user by email — delegates to the repository.
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetUserByEmailAsync(email);
    }
}