using Spaartje.Domain.Models;

namespace Spaartje.DAL.Repositories;

public interface IGroupRepository
{
    Task<List<Group>> GetGroupsForUserAsync(string userId);
    Task<Group?> GetByIdAsync(int id);
    Task AddAsync(Group group);
    Task UpdateAsync(Group group);
    Task DeleteAsync(Group group);
    Task AddMemberAsync(GroupMember member);
    Task RemoveMemberAsync(GroupMember member);
    Task<GroupMember?> GetMemberAsync(int groupId, string userId);
    Task AddTransactionAsync(GroupTransaction transaction);
    Task<List<GroupTransaction>> GetTransactionsByGroupIdAsync(int groupId);
}