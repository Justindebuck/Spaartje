using Spaartje.DAL.Repositories;
using Spaartje.Domain.Models;

namespace Spaartje.BLL.Services;


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

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetUserByIdAsync(id);
    }

     public async Task<User?> ValidateLoginAsync(string email, string password)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password)) return null;

        return user;
    }

     public async Task<(bool Success, string Error)> RegisterAsync(string email, string userName, string password)
    {
        if (await _userRepository.EmailExistsAsync(email))
            return (false, "An account with this email already exists.");

        var user = new User
        {
            Email     = email,
            UserName  = userName,
            Password  = BCrypt.Net.BCrypt.HashPassword(password),
            Role      = "User",
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddUserAsync(user);
        return (true, string.Empty);
    }
}