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
    // TEST 1: GetAllUsersAsync returns all users
    // ─────────────────────────────────────────────

    // [Fact] marks this method as a test.
    // xUnit will find and run any method with [Fact].
    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        // ── ARRANGE ──────────────────────────────
        // Create a fake list of users to return from the mock.
        var fakeUsers = new List<User>
        {
            new User { Id = "1", Email = "alice@test.com", Roles = new List<string>() },
            new User { Id = "2", Email = "bob@test.com",   Roles = new List<string> { "Admin" } }
        };

        // Create a mock of IUserRepository.
        // Mock<T> creates a fake object that implements the interface.
        var mockRepo = new Mock<IUserRepository>();

        // Setup: "when GetAllUsersAsync() is called, return fakeUsers"
        // ReturnsAsync wraps the value in a completed Task automatically.
        mockRepo.Setup(r => r.GetAllUsersAsync())
                .ReturnsAsync(fakeUsers);

        // Create the real UserService, passing the mock as its dependency.
        // UserService doesn't know it's talking to a fake — it just calls the interface.
        var service = new UserService(mockRepo.Object);

        // ── ACT ──────────────────────────────────
        // Call the method we are testing.
        var result = await service.GetAllUsersAsync();

        // ── ASSERT ───────────────────────────────
        // Assert.Equal checks that two values are equal.
        // "I expect the result to contain 2 users"
        Assert.Equal(2, result.Count);

        // Check specific values in the result.
        Assert.Equal("alice@test.com", result[0].Email);
        Assert.Equal("bob@test.com",   result[1].Email);
    }

    // ─────────────────────────────────────────────
    // TEST 2: GetAllUsersAsync with empty database
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetAllUsersAsync_WhenNoUsers_ReturnsEmptyList()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        // Return an empty list — simulates a fresh database with no users.
        mockRepo.Setup(r => r.GetAllUsersAsync())
                .ReturnsAsync(new List<User>());

        var service = new UserService(mockRepo.Object);

        // Act
        var result = await service.GetAllUsersAsync();

        // Assert
        // Assert.Empty checks that the list has zero items.
        Assert.Empty(result);
    }

    // ─────────────────────────────────────────────
    // TEST 3: GetUserByEmailAsync returns correct user
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ReturnsUser()
    {
        // Arrange
        var fakeUser = new User
        {
            Id = "1",
            Email = "alice@test.com",
            Roles = new List<string>()
        };

        var mockRepo = new Mock<IUserRepository>();

        // Setup: "when GetUserByEmailAsync is called WITH "alice@test.com", return fakeUser"
        // It.Is<string>(...) means "match only when the argument satisfies this condition"
        mockRepo.Setup(r => r.GetUserByEmailAsync(It.Is<string>(e => e == "alice@test.com")))
                .ReturnsAsync(fakeUser);

        var service = new UserService(mockRepo.Object);

        // Act
        var result = await service.GetUserByEmailAsync("alice@test.com");

        // Assert
        // Assert.NotNull checks the result is not null.
        Assert.NotNull(result);
        Assert.Equal("alice@test.com", result.Email);
    }

    // ─────────────────────────────────────────────
    // TEST 4: GetUserByEmailAsync with unknown email
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetUserByEmailAsync_WithUnknownEmail_ReturnsNull()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        // Return null — simulates "user not found in database"
        mockRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

        var service = new UserService(mockRepo.Object);

        // Act
        var result = await service.GetUserByEmailAsync("nobody@test.com");

        // Assert
        // Assert.Null checks the result IS null.
        Assert.Null(result);
    }
}