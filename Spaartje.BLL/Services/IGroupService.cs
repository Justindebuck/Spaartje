using Spaartje.Domain.Models;

namespace Spaartje.BLL.Services;

public interface IGroupService
{
    Task<List<Group>> GetGroupsForUserAsync(int userId);
    Task<Group?> GetGroupByIdAsync(int groupId, int userId);
    Task<Group> CreateGroupAsync(string name, decimal? budgetLimit, int ownerId);
    Task DeleteGroupAsync(int groupId, int userId);
    Task<string?> AddMemberByEmailAsync(int groupId, string email, int requestingUserId);
    Task RemoveMemberAsync(int groupId, int memberUserId, int requestingUserId);
    Task<string?> AddTransactionAsync(int groupId, decimal amount, string description, DateTime date, TransactionType type, int userId);
    Task<List<GroupTransaction>> GetTransactionsAsync(int groupId, int userId);
    Task<string?> UpdateGroupAsync(int groupId, string name, decimal? budgetLimit, int userId);
}