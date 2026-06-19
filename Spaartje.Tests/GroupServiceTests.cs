using Moq;
using Xunit;
using Spaartje.BLL.Services;
using Spaartje.DAL.Repositories;
using Spaartje.Domain.Models;

namespace Spaartje.Tests;

public class GroupServiceTests
{
    // ─────────────────────────────────────────────
    // TEST 1: GetGroupsForUserAsync returns correct groups
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetGroupsForUserAsync_ReturnsGroupsForThatUser()
    {
        // Arrange
        var userId = 1;

        var fakeGroups = new List<Group>
        {
            new Group { Id = 10, Name = "Roommates", OwnerId = userId },
            new Group { Id = 11, Name = "Road Trip", OwnerId = userId }
        };

        var mockGroupRepo = new Mock<IGroupRepository>();
        mockGroupRepo.Setup(r => r.GetGroupsForUserAsync(userId))
                     .ReturnsAsync(fakeGroups);

        // GroupService needs two repositories in its constructor
        var mockUserRepo = new Mock<IUserRepository>();

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act
        var result = await service.GetGroupsForUserAsync(userId);

        // Assert
        Assert.Equal(2, result.Count);
    }

    // ─────────────────────────────────────────────
    // TEST 2: GetGroupsForUserAsync with no groups
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetGroupsForUserAsync_WhenNoGroups_ReturnsEmptyList()
    {
        // Arrange
        var mockGroupRepo = new Mock<IGroupRepository>();

        // Return an empty list — simulates a user with no groups yet
        mockGroupRepo.Setup(r => r.GetGroupsForUserAsync(It.IsAny<int>()))
                     .ReturnsAsync(new List<Group>());

        var mockUserRepo = new Mock<IUserRepository>();

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act
        var result = await service.GetGroupsForUserAsync(99);

        // Assert
        Assert.Empty(result);
    }

    // ─────────────────────────────────────────────
    // TEST 3: GetGroupByIdAsync — group found and user is owner
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetGroupByIdAsync_WhenGroupExistsAndUserIsOwner_ReturnsGroup()
    {
        // Arrange
        var group = new Group
        {
            Id      = 10,
            Name    = "Roommates",
            OwnerId = 1, // user 1 is the owner
            Members = new List<GroupMember>()
        };

        var mockGroupRepo = new Mock<IGroupRepository>();
        mockGroupRepo.Setup(r => r.GetByIdAsync(10))
                     .ReturnsAsync(group);

        var mockUserRepo = new Mock<IUserRepository>();

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act — user 1 is the owner so they should get the group back
        var result = await service.GetGroupByIdAsync(10, userId: 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10,          result!.Id);
        Assert.Equal("Roommates", result.Name);
    }

    // ─────────────────────────────────────────────
    // TEST 4: GetGroupByIdAsync — user is not a member, returns null
    // GroupService checks if the user is the owner OR a member
    // If neither, it returns null instead of the group
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetGroupByIdAsync_WhenUserIsNotMember_ReturnsNull()
    {
        // Arrange
        var group = new Group
        {
            Id      = 10,
            OwnerId = 1, // owned by user 1
            Members = new List<GroupMember>() // no other members
        };

        var mockGroupRepo = new Mock<IGroupRepository>();
        mockGroupRepo.Setup(r => r.GetByIdAsync(10))
                     .ReturnsAsync(group);

        var mockUserRepo = new Mock<IUserRepository>();

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act — user 99 is not the owner and not a member
        var result = await service.GetGroupByIdAsync(10, userId: 99);

        // Assert
        Assert.Null(result);
    }

    // ─────────────────────────────────────────────
    // TEST 5: GetGroupByIdAsync — group not found
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetGroupByIdAsync_WhenGroupDoesNotExist_ReturnsNull()
    {
        // Arrange
        var mockGroupRepo = new Mock<IGroupRepository>();

        // Simulate: group not found in database
        mockGroupRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                     .ReturnsAsync((Group?)null);

        var mockUserRepo = new Mock<IUserRepository>();

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act
        var result = await service.GetGroupByIdAsync(999, userId: 1);

        // Assert
        Assert.Null(result);
    }

    // ─────────────────────────────────────────────
    // TEST 6: CreateGroupAsync calls AddAsync exactly once
    // ─────────────────────────────────────────────

    [Fact]
    public async Task CreateGroupAsync_WithValidData_CallsAddAsyncOnce()
    {
        // Arrange
        var mockGroupRepo = new Mock<IGroupRepository>();

        mockGroupRepo.Setup(r => r.AddAsync(It.IsAny<Group>()))
                     .Returns(Task.CompletedTask);

        mockGroupRepo.Setup(r => r.AddMemberAsync(It.IsAny<GroupMember>()))
                     .Returns(Task.CompletedTask);

        var mockUserRepo = new Mock<IUserRepository>();

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act
        await service.CreateGroupAsync("Roommates", budgetLimit: 500m, ownerId: 1);

        // Assert
        // Verify that AddAsync was called exactly once — proves the group was saved
        mockGroupRepo.Verify(r => r.AddAsync(It.IsAny<Group>()), Times.Once);
    }

    // ─────────────────────────────────────────────
    // TEST 7: CreateGroupAsync adds the owner as a member automatically
    // ─────────────────────────────────────────────

    [Fact]
    public async Task CreateGroupAsync_AutomaticallyAddsOwnerAsMember()
    {
        // Arrange
        var mockGroupRepo = new Mock<IGroupRepository>();
        GroupMember? addedMember = null;

        mockGroupRepo.Setup(r => r.AddAsync(It.IsAny<Group>()))
                     .Returns(Task.CompletedTask);

        // Callback captures the GroupMember that gets passed to AddMemberAsync
        // so we can inspect it in the Assert
        mockGroupRepo.Setup(r => r.AddMemberAsync(It.IsAny<GroupMember>()))
                     .Callback<GroupMember>(m => addedMember = m)
                     .Returns(Task.CompletedTask);

        var mockUserRepo = new Mock<IUserRepository>();

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act
        await service.CreateGroupAsync("Roommates", budgetLimit: null, ownerId: 1);

        // Assert — the captured member must be the owner (userId = 1)
        Assert.NotNull(addedMember);
        Assert.Equal(1, addedMember!.UserId);
    }

    // ─────────────────────────────────────────────
    // TEST 8: DeleteGroupAsync — owner can delete
    // ─────────────────────────────────────────────

    [Fact]
    public async Task DeleteGroupAsync_WhenCallerIsOwner_CallsDeleteAsync()
    {
        // Arrange
        var group = new Group { Id = 10, OwnerId = 1, Members = new List<GroupMember>() };

        var mockGroupRepo = new Mock<IGroupRepository>();
        mockGroupRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(group);
        mockGroupRepo.Setup(r => r.DeleteAsync(group)).Returns(Task.CompletedTask);

        var mockUserRepo = new Mock<IUserRepository>();

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act — caller IS the owner (userId = 1)
        await service.DeleteGroupAsync(groupId: 10, userId: 1);

        // Assert
        // Verify that DeleteAsync was called exactly once — proves the delete happened
        mockGroupRepo.Verify(r => r.DeleteAsync(group), Times.Once);
    }

    // ─────────────────────────────────────────────
    // TEST 9: DeleteGroupAsync — non-owner CANNOT delete
    // This tests the business rule: only the owner can delete a group
    // ─────────────────────────────────────────────

    [Fact]
    public async Task DeleteGroupAsync_WhenCallerIsNotOwner_DoesNotDelete()
    {
        // Arrange
        var group = new Group { Id = 10, OwnerId = 1, Members = new List<GroupMember>() };

        var mockGroupRepo = new Mock<IGroupRepository>();
        mockGroupRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(group);

        var mockUserRepo = new Mock<IUserRepository>();

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act — caller is user 2, NOT the owner
        await service.DeleteGroupAsync(groupId: 10, userId: 2);

        // Assert
        // Verify that DeleteAsync was NEVER called
        // Times.Never means "this should not have been called at all"
        mockGroupRepo.Verify(r => r.DeleteAsync(It.IsAny<Group>()), Times.Never);
    }

    // ─────────────────────────────────────────────
    // TEST 10: DeleteGroupAsync — group does not exist
    // ─────────────────────────────────────────────

    [Fact]
    public async Task DeleteGroupAsync_WhenGroupDoesNotExist_DoesNothing()
    {
        // Arrange
        var mockGroupRepo = new Mock<IGroupRepository>();

        // Simulate: group not found in database
        mockGroupRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                     .ReturnsAsync((Group?)null);

        var mockUserRepo = new Mock<IUserRepository>();

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act
        await service.DeleteGroupAsync(groupId: 999, userId: 1);

        // Assert
        // Delete should never be called if the group doesn't exist
        mockGroupRepo.Verify(r => r.DeleteAsync(It.IsAny<Group>()), Times.Never);
    }

    // ─────────────────────────────────────────────
    // TEST 11: AddMemberByEmailAsync — owner invites a valid email
    // ─────────────────────────────────────────────

    [Fact]
    public async Task AddMemberByEmailAsync_WithValidEmailAndOwner_AddsMember()
    {
        // Arrange
        var group = new Group { Id = 10, OwnerId = 1, Members = new List<GroupMember>() };
        var bob   = new User  { Id = 2, Email = "bob@test.com" };

        var mockGroupRepo = new Mock<IGroupRepository>();
        mockGroupRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(group);
        mockGroupRepo.Setup(r => r.GetMemberAsync(10, 2)).ReturnsAsync((GroupMember?)null);
        mockGroupRepo.Setup(r => r.AddMemberAsync(It.IsAny<GroupMember>())).Returns(Task.CompletedTask);

        // UserRepository is used to look up the user by email
        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetUserByEmailAsync("bob@test.com")).ReturnsAsync(bob);

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act — owner (1) invites bob by email
        var result = await service.AddMemberByEmailAsync(groupId: 10, email: "bob@test.com", requestingUserId: 1);

        // Assert
        // Returns null when it works — same pattern as TransactionService
        Assert.Null(result);

        // AddMemberAsync must have been called with bob's userId (2)
        mockGroupRepo.Verify(r => r.AddMemberAsync(It.Is<GroupMember>(m => m.UserId == 2)), Times.Once);
    }

    // ─────────────────────────────────────────────
    // TEST 12: AddMemberByEmailAsync — non-owner CANNOT invite
    // This tests the business rule: only the owner can add members
    // ─────────────────────────────────────────────

    [Fact]
    public async Task AddMemberByEmailAsync_WhenCallerIsNotOwner_ReturnsError()
    {
        // Arrange
        var group = new Group { Id = 10, OwnerId = 1, Members = new List<GroupMember>() };

        var mockGroupRepo = new Mock<IGroupRepository>();
        mockGroupRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(group);

        var mockUserRepo = new Mock<IUserRepository>();

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act — user 3 is NOT the owner
        var result = await service.AddMemberByEmailAsync(groupId: 10, email: "bob@test.com", requestingUserId: 3);

        // Assert
        // Returns an error message when permission is denied
        Assert.NotNull(result);

        // AddMemberAsync must never have been called
        mockGroupRepo.Verify(r => r.AddMemberAsync(It.IsAny<GroupMember>()), Times.Never);
    }

    // ─────────────────────────────────────────────
    // TEST 13: AddMemberByEmailAsync — user already in the group
    // ─────────────────────────────────────────────

    [Fact]
    public async Task AddMemberByEmailAsync_WhenUserAlreadyMember_ReturnsError()
    {
        // Arrange
        var group     = new Group      { Id = 10, OwnerId = 1, Members = new List<GroupMember>() };
        var bob       = new User       { Id = 2, Email = "bob@test.com" };
        var bobMember = new GroupMember { GroupId = 10, UserId = 2 }; // bob is already here

        var mockGroupRepo = new Mock<IGroupRepository>();
        mockGroupRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(group);
        // GetMemberAsync returns the existing member — bob is already in the group
        mockGroupRepo.Setup(r => r.GetMemberAsync(10, 2)).ReturnsAsync(bobMember);

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetUserByEmailAsync("bob@test.com")).ReturnsAsync(bob);

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act — owner tries to add bob who is already a member
        var result = await service.AddMemberByEmailAsync(groupId: 10, email: "bob@test.com", requestingUserId: 1);

        // Assert — returns an error message
        Assert.NotNull(result);

        // AddMemberAsync must never have been called
        mockGroupRepo.Verify(r => r.AddMemberAsync(It.IsAny<GroupMember>()), Times.Never);
    }

    // ─────────────────────────────────────────────
    // TEST 14: AddMemberByEmailAsync — unknown email
    // ─────────────────────────────────────────────

    [Fact]
    public async Task AddMemberByEmailAsync_WithUnknownEmail_ReturnsError()
    {
        // Arrange
        var group = new Group { Id = 10, OwnerId = 1, Members = new List<GroupMember>() };

        var mockGroupRepo = new Mock<IGroupRepository>();
        mockGroupRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(group);

        var mockUserRepo = new Mock<IUserRepository>();
        // Simulate: no user found for this email
        mockUserRepo.Setup(r => r.GetUserByEmailAsync("nobody@test.com"))
                    .ReturnsAsync((User?)null);

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act
        var result = await service.AddMemberByEmailAsync(groupId: 10, email: "nobody@test.com", requestingUserId: 1);

        // Assert — returns an error message
        Assert.NotNull(result);

        // AddMemberAsync must never have been called
        mockGroupRepo.Verify(r => r.AddMemberAsync(It.IsAny<GroupMember>()), Times.Never);
    }

    // ─────────────────────────────────────────────
    // TEST 15: RemoveMemberAsync — owner removes a member
    // ─────────────────────────────────────────────

    [Fact]
    public async Task RemoveMemberAsync_WhenCallerIsOwner_RemovesMember()
    {
        // Arrange
        var group     = new Group      { Id = 10, OwnerId = 1, Members = new List<GroupMember>() };
        var bobMember = new GroupMember { GroupId = 10, UserId = 2 };

        var mockGroupRepo = new Mock<IGroupRepository>();
        mockGroupRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(group);
        // GetMemberAsync returns bob's member record
        mockGroupRepo.Setup(r => r.GetMemberAsync(10, 2)).ReturnsAsync(bobMember);
        mockGroupRepo.Setup(r => r.RemoveMemberAsync(It.IsAny<GroupMember>())).Returns(Task.CompletedTask);

        var mockUserRepo = new Mock<IUserRepository>();

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act — owner (1) removes bob (2)
        await service.RemoveMemberAsync(groupId: 10, memberUserId: 2, requestingUserId: 1);

        // Assert
        // Verify RemoveMemberAsync was called with bob's member object
        mockGroupRepo.Verify(r => r.RemoveMemberAsync(It.Is<GroupMember>(m => m.UserId == 2)), Times.Once);
    }

    // ─────────────────────────────────────────────
    // TEST 16: RemoveMemberAsync — non-owner CANNOT remove
    // This tests the business rule: only the owner can remove members
    // ─────────────────────────────────────────────

    [Fact]
    public async Task RemoveMemberAsync_WhenCallerIsNotOwner_DoesNotRemove()
    {
        // Arrange
        var group = new Group { Id = 10, OwnerId = 1, Members = new List<GroupMember>() };

        var mockGroupRepo = new Mock<IGroupRepository>();
        mockGroupRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(group);

        var mockUserRepo = new Mock<IUserRepository>();

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act — user 3 is NOT the owner
        await service.RemoveMemberAsync(groupId: 10, memberUserId: 2, requestingUserId: 3);

        // Assert
        // Verify that RemoveMemberAsync was NEVER called
        mockGroupRepo.Verify(r => r.RemoveMemberAsync(It.IsAny<GroupMember>()), Times.Never);
    }

    // ─────────────────────────────────────────────
    // TEST 17: RemoveMemberAsync — owner cannot remove themselves
    // ─────────────────────────────────────────────

    [Fact]
    public async Task RemoveMemberAsync_OwnerCannotRemoveThemself()
    {
        // Arrange
        var group = new Group { Id = 10, OwnerId = 1, Members = new List<GroupMember>() };

        var mockGroupRepo = new Mock<IGroupRepository>();
        mockGroupRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(group);

        var mockUserRepo = new Mock<IUserRepository>();

        var service = new GroupService(mockGroupRepo.Object, mockUserRepo.Object);

        // Act — owner tries to remove themselves
        await service.RemoveMemberAsync(groupId: 10, memberUserId: 1, requestingUserId: 1);

        // Assert
        // The owner cannot remove themselves — RemoveMemberAsync must never be called
        mockGroupRepo.Verify(r => r.RemoveMemberAsync(It.IsAny<GroupMember>()), Times.Never);
    }
}