using Spaartje.Domain.Models;

namespace Spaartje.BLL.Services;

public interface IGroupService
{
    Task<List<Group>> GetGroupsForUserAsync(string userId);
    Task<Group?> GetGroupByIdAsync(int groupId, string userId);
    Task<Group> CreateGroupAsync(string name, decimal? budgetLimit, string ownerId);
    Task DeleteGroupAsync(int groupId, string userId);
    Task<string?> AddMemberByEmailAsync(int groupId, string email, string requestingUserId);
    Task RemoveMemberAsync(int groupId, string memberUserId, string requestingUserId);
    Task<string?> AddTransactionAsync(int groupId, decimal amount, string description, DateTime date, TransactionType type, string userId);
    Task<List<GroupTransaction>> GetTransactionsAsync(int groupId, string userId);
}