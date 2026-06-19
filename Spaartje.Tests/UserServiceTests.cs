// Moq is the mocking library
using Moq;
// xUnit provides [Fact] and Assert
using Xunit;
using Spaartje.BLL.Services;
using Spaartje.DAL.Repositories;
using Spaartje.Domain.Models;

namespace Spaartje.Tests;

// The class name describes WHAT we are testing.
// Convention: [ClassName]Tests
public class UserServiceTests

{
    // ─────────────────────────────────────────────
    // TEST 1: RegisterAsync with a new email succeeds
    // ─────────────────────────────────────────────
    
    [Fact]
    public async Task RegisterAsync_WithNewEmail_ReturnsSuccess()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        // EmailExistsAsync returns false → the email is not taken yet
        mockRepo.Setup(r => r.EmailExistsAsync("alice@test.com"))
                .ReturnsAsync(false);

        mockRepo.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

        var service = new UserService(mockRepo.Object);

        // Act
        var (success, error) = await service.RegisterAsync("alice@test.com", "alice", "password123");

        // Assert
        Assert.True(success);
        Assert.Equal(string.Empty, error);
    }

    // ─────────────────────────────────────────────
    // TEST 2: RegisterAsync with a duplicate email fails
    // ─────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        // EmailExistsAsync returns true → email is already taken
        mockRepo.Setup(r => r.EmailExistsAsync("alice@test.com"))
                .ReturnsAsync(true);

        var service = new UserService(mockRepo.Object);

        // Act
        var (success, error) = await service.RegisterAsync("alice@test.com", "alice", "password123");

        // Assert
        Assert.False(success);
        Assert.NotEmpty(error); // some error message must be returned
    }

    // ─────────────────────────────────────────────
    // TEST 3: Password is never stored as plain text
    // This is the most important security test
    // ─────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_WithNewEmail_NeverStoresPlainTextPassword()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        User? savedUser = null;

        mockRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

        // Callback captures whatever User object gets passed to AddUserAsync
        mockRepo.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                .Callback<User>(u => savedUser = u)
                .Returns(Task.CompletedTask);

        var service = new UserService(mockRepo.Object);

        // Act
        await service.RegisterAsync("alice@test.com", "alice", "myplainpassword");

        // Assert
        Assert.NotNull(savedUser);

        // The stored password must NOT be the plain text original
        Assert.NotEqual("myplainpassword", savedUser!.Password);

        // BCrypt hashes always start with $2a$ or $2b$
        Assert.StartsWith("$2", savedUser.Password);
    }

    // ─────────────────────────────────────────────
    // TEST 4: New users always get the "User" role
    // ─────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_WithNewEmail_AssignsUserRole()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        User? savedUser = null;

        mockRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

        mockRepo.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                .Callback<User>(u => savedUser = u)
                .Returns(Task.CompletedTask);

        var service = new UserService(mockRepo.Object);

        // Act
        await service.RegisterAsync("alice@test.com", "alice", "password123");

        // Assert
        Assert.NotNull(savedUser);
        // Role is now a single string — not a List<string> anymore
        Assert.Equal("User", savedUser!.Role);
    }

    // ─────────────────────────────────────────────
    // TEST 5: AddUserAsync is never called when email is taken
    // ─────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_NeverCallsAddUserAsync()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        mockRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

        var service = new UserService(mockRepo.Object);

        // Act
        await service.RegisterAsync("alice@test.com", "alice", "password123");

        // Assert
        // Times.Never means AddUserAsync must not have been called at all
        mockRepo.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Never);
    }

    // ─────────────────────────────────────────────
    // TEST 6: Correct credentials return the user
    // We use a real BCrypt hash so Verify() actually runs
    // ─────────────────────────────────────────────

    [Fact]
    public async Task ValidateLoginAsync_WithCorrectCredentials_ReturnsUser()
    {
        // Arrange
        // Create a real hash — BCrypt.Verify() needs an actual hash to compare against
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");

        var fakeUser = new User
        {
            Id       = 1,              // int — not a string anymore
            Email    = "alice@test.com",
            Password = hashedPassword,
            UserName = "alice",
            Role     = "User"          // single string — not a List<string> anymore
        };

        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetUserByEmailAsync("alice@test.com"))
                .ReturnsAsync(fakeUser);

        var service = new UserService(mockRepo.Object);

        // Act
        var result = await service.ValidateLoginAsync("alice@test.com", "correctpassword");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    // ─────────────────────────────────────────────
    // TEST 7: Wrong password returns null
    // ─────────────────────────────────────────────

    [Fact]
    public async Task ValidateLoginAsync_WithWrongPassword_ReturnsNull()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");

        var fakeUser = new User
        {
            Id       = 1,  // int — not a string anymore
            Email    = "alice@test.com",
            Password = hashedPassword,
            Role     = "User"
        };

        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetUserByEmailAsync("alice@test.com"))
                .ReturnsAsync(fakeUser);

        var service = new UserService(mockRepo.Object);

        // Act
        var result = await service.ValidateLoginAsync("alice@test.com", "wrongpassword");

        // Assert
        // BCrypt.Verify returns false for a wrong password → service returns null
        Assert.Null(result);
    }

    // ─────────────────────────────────────────────
    // TEST 8: Unknown email returns null
    // ─────────────────────────────────────────────

    [Fact]
    public async Task ValidateLoginAsync_WithUnknownEmail_ReturnsNull()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        // Simulate: no user found for this email
        mockRepo.Setup(r => r.GetUserByEmailAsync("unknown@test.com"))
                .ReturnsAsync((User?)null);

        var service = new UserService(mockRepo.Object);

        // Act
        var result = await service.ValidateLoginAsync("unknown@test.com", "anypassword");

        // Assert
        Assert.Null(result);
    }
}