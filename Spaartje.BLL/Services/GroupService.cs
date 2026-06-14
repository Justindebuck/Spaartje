
using Spaartje.DAL.Repositories;
using Spaartje.Domain.Models;

namespace Spaartje.BLL.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;

    public GroupService(IGroupRepository groupRepository, IUserRepository userRepository)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
    }

    public async Task<List<Group>> GetGroupsForUserAsync(int userId)
    {
        return await _groupRepository.GetGroupsForUserAsync(userId);
    }

    public async Task<Group?> GetGroupByIdAsync(int groupId, int userId)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);
        if (group == null) return null;

        // Check if the user is the owner or a member
        var isMember = group.OwnerId == userId ||
                       group.Members.Any(m => m.UserId == userId);

        if (!isMember) return null;

        return group;
    }

    public async Task<Group> CreateGroupAsync(string name, decimal? budgetLimit, int ownerId)
    {
        // Build the group object
        var group = new Group
        {
            Name        = name,
            BudgetLimit = budgetLimit,
            OwnerId     = ownerId,
            CreatedAt   = DateTime.UtcNow
        };

        await _groupRepository.AddAsync(group);

        // Add the creator as a member so they appear in the members list
        var ownerMember = new GroupMember
        {
            GroupId  = group.Id,
            UserId   = ownerId,
            JoinedAt = DateTime.UtcNow
        };

        await _groupRepository.AddMemberAsync(ownerMember);

        return group;
    }

    public async Task DeleteGroupAsync(int groupId, int userId)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);

        // Only the owner can delete the group
        if (group == null || group.OwnerId != userId)
            return;

        await _groupRepository.DeleteAsync(group);
    }

    // Returns an error message if something goes wrong
    // Returns null if it worked fine — same pattern as TransactionService
    public async Task<string?> AddMemberByEmailAsync(int groupId, string email, int requestingUserId)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);

        // Only the owner can invite members
        if (group == null || group.OwnerId != requestingUserId)
            return "You do not have permission to invite members.";

        // Search for the user by email using Identity
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null)
            return "No account found with that email address.";

        // Check if they are already in the group
        var existing = await _groupRepository.GetMemberAsync(groupId, user.Id);
        if (existing != null)
            return "That user is already a member of this group.";

        var member = new GroupMember
        {
            GroupId  = groupId,
            UserId   = user.Id,
            JoinedAt = DateTime.UtcNow
        };

        await _groupRepository.AddMemberAsync(member);
        return null;
    }

    public async Task RemoveMemberAsync(int groupId, int memberUserId, int requestingUserId)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);

        // Only the owner can remove members
        if (group == null || group.OwnerId != requestingUserId)
            return;

        // The owner cannot remove themselves
        if (memberUserId == group.OwnerId)
            return;

        var member = await _groupRepository.GetMemberAsync(groupId, memberUserId);
        if (member == null) return;

        await _groupRepository.RemoveMemberAsync(member);
    }

    // Returns an error message if something goes wrong
    // Returns null if it worked fine
    public async Task<string?> AddTransactionAsync(int groupId, decimal amount, string description, DateTime date, TransactionType type, int userId)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);
        if (group == null) return "Group not found.";

        // Check the user is a member of this group
        var isMember = group.OwnerId == userId ||
                       group.Members.Any(m => m.UserId == userId);

        if (!isMember)
            return "You are not a member of this group.";

        if (amount <= 0)
            return "Amount must be greater than zero.";

        var transaction = new GroupTransaction
        {
            GroupId     = groupId,
            Amount      = amount,
            Description = description,
            Date        = date,
            Type        = type,
            UserId      = userId
        };

        await _groupRepository.AddTransactionAsync(transaction);
        return null;
    }

    public async Task<List<GroupTransaction>> GetTransactionsAsync(int groupId, int userId)
    {
        var group = await _groupRepository.GetByIdAsync(groupId);
        if (group == null) return new List<GroupTransaction>();

        // Check the user is a member
        var isMember = group.OwnerId == userId ||
                       group.Members.Any(m => m.UserId == userId);

        if (!isMember) return new List<GroupTransaction>();

        return await _groupRepository.GetTransactionsByGroupIdAsync(groupId);
    }
}