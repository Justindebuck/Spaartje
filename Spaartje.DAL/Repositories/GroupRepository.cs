using Microsoft.EntityFrameworkCore;
using Spaartje.DAL.Data;
using Spaartje.Domain.Models;

namespace Spaartje.DAL.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly ApplicationDbContext _context;

    public GroupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Group>> GetGroupsForUserAsync(int userId)
    {
        // Get groups where the user is the owner
        var ownedGroups = await _context.Groups
            .Where(g => g.OwnerId == userId)
            .Include(g => g.Members)
            .ToListAsync();

        // Get group IDs where the user is a member
        var memberGroupIds = await _context.GroupMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.GroupId)
            .ToListAsync();

        // Get those groups
        var memberGroups = await _context.Groups
            .Where(g => memberGroupIds.Contains(g.Id))
            .Include(g => g.Members)
            .ToListAsync();

        // Combine both lists and remove any duplicates
        var allGroups = ownedGroups
            .Union(memberGroups)
            .OrderBy(g => g.Name)
            .ToList();

        return allGroups;
    }

    public async Task<Group?> GetByIdAsync(int id)
    {
        // Always load members and transactions together with the group
        return await _context.Groups
            .Include(g => g.Members)
            .Include(g => g.Transactions)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task AddAsync(Group group)
    {
        await _context.Groups.AddAsync(group);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Group group)
    {
        _context.Groups.Update(group);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Group group)
    {
        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
    }

    public async Task AddMemberAsync(GroupMember member)
    {
        await _context.GroupMembers.AddAsync(member);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(GroupMember member)
    {
        _context.GroupMembers.Remove(member);
        await _context.SaveChangesAsync();
    }

    public async Task<GroupMember?> GetMemberAsync(int groupId, int userId)
    {
        return await _context.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
    }

    public async Task AddTransactionAsync(GroupTransaction transaction)
    {
        await _context.GroupTransactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<List<GroupTransaction>> GetTransactionsByGroupIdAsync(int groupId)
    {
        return await _context.GroupTransactions
            .Where(t => t.GroupId == groupId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }
}