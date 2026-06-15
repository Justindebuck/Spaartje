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
       var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT DISTINCT g.Id, g.Name, g.BudgetLimit, g.OwnerId, g.CreatedAt
            FROM Groups g
            LEFT JOIN GroupMembers gm ON gm.GroupId = g.Id
            WHERE g.OwnerId = @userId OR gm.UserId = @userId
            ORDER BY g.Name";

        var param = command.CreateParameter();
        param.ParameterName = "@userId";
        param.Value = userId;
        command.Parameters.Add(param);

        using var reader = await command.ExecuteReaderAsync();

        var groups = new List<Group>();
        while (await reader.ReadAsync())
        {
            groups.Add(new Group
            {
                Id          = reader.GetInt32(0),
                Name        = reader.GetString(1),
                BudgetLimit = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                OwnerId     = reader.GetInt32(3),
                CreatedAt   = reader.GetDateTime(4),
                Members     = new List<GroupMember>()
            });
        }

        return groups;
    }

    public async Task<Group?> GetByIdAsync(int id)
    {
          var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        Group? group = null;

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT Id, Name, BudgetLimit, OwnerId, CreatedAt
                FROM Groups
                WHERE Id = @id";

            var param = command.CreateParameter();
            param.ParameterName = "@id";
            param.Value = id;
            command.Parameters.Add(param);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                group = new Group
                {
                    Id          = reader.GetInt32(0),
                    Name        = reader.GetString(1),
                    BudgetLimit = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    OwnerId     = reader.GetInt32(3),
                    CreatedAt   = reader.GetDateTime(4)
                };
            }
        }

        if (group == null) return null;

        // Load members and transactions separately
        group.Members      = await GetMembersForGroupAsync(id);
        group.Transactions = await GetTransactionsByGroupIdAsync(id);

        return group;
    }

    public async Task AddAsync(Group group)
    {
      var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Groups (Name, BudgetLimit, OwnerId, CreatedAt)
            VALUES (@name, @budgetLimit, @ownerId, @createdAt);
            SELECT SCOPE_IDENTITY();";

        void AddParam(string name, object? value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            command.Parameters.Add(p);
        }

        AddParam("@name",        group.Name);
        AddParam("@budgetLimit", group.BudgetLimit);
        AddParam("@ownerId",     group.OwnerId);
        AddParam("@createdAt",   group.CreatedAt);

        // SCOPE_IDENTITY() returns the new Id so we can use it immediately
        var newId = await command.ExecuteScalarAsync();
        group.Id = Convert.ToInt32(newId);
    }

    public async Task UpdateAsync(Group group)
    {
       var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Groups
            SET Name        = @name,
                BudgetLimit = @budgetLimit
            WHERE Id = @id";

        void AddParam(string name, object? value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            command.Parameters.Add(p);
        }

        AddParam("@name",        group.Name);
        AddParam("@budgetLimit", group.BudgetLimit);
        AddParam("@id",          group.Id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Group group)
    {
       var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Groups WHERE Id = @id";

        var param = command.CreateParameter();
        param.ParameterName = "@id";
        param.Value = group.Id;
        command.Parameters.Add(param);

        await command.ExecuteNonQueryAsync();
    }

    public async Task AddMemberAsync(GroupMember member)
    {
       var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO GroupMembers (GroupId, UserId, JoinedAt)
            VALUES (@groupId, @userId, @joinedAt)";

        void AddParam(string name, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            command.Parameters.Add(p);
        }

        AddParam("@groupId",  member.GroupId);
        AddParam("@userId",   member.UserId);
        AddParam("@joinedAt", member.JoinedAt);

        await command.ExecuteNonQueryAsync();
    }

    public async Task RemoveMemberAsync(GroupMember member)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM GroupMembers WHERE GroupId = @groupId AND UserId = @userId";

        void AddParam(string name, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            command.Parameters.Add(p);
        }

        AddParam("@groupId", member.GroupId);
        AddParam("@userId",  member.UserId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<GroupMember?> GetMemberAsync(int groupId, int userId)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT GroupId, UserId, JoinedAt
            FROM GroupMembers
            WHERE GroupId = @groupId AND UserId = @userId";

        void AddParam(string name, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            command.Parameters.Add(p);
        }

        AddParam("@groupId", groupId);
        AddParam("@userId",  userId);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new GroupMember
            {
                GroupId  = reader.GetInt32(0),
                UserId   = reader.GetInt32(1),
                JoinedAt = reader.GetDateTime(2)
            };
        }

        return null;
    }

    public async Task AddTransactionAsync(GroupTransaction transaction)
    {
       var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO GroupTransactions (GroupId, Amount, Description, Date, Type, UserId)
            VALUES (@groupId, @amount, @description, @date, @type, @userId)";

        void AddParam(string name, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            command.Parameters.Add(p);
        }

        AddParam("@groupId",     transaction.GroupId);
        AddParam("@amount",      transaction.Amount);
        AddParam("@description", transaction.Description);
        AddParam("@date",        transaction.Date);
        AddParam("@type",        (int)transaction.Type);
        AddParam("@userId",      transaction.UserId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<GroupTransaction>> GetTransactionsByGroupIdAsync(int groupId)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, GroupId, Amount, Description, Date, Type, UserId
            FROM GroupTransactions
            WHERE GroupId = @groupId
            ORDER BY Date DESC";

        var param = command.CreateParameter();
        param.ParameterName = "@groupId";
        param.Value = groupId;
        command.Parameters.Add(param);

        using var reader = await command.ExecuteReaderAsync();

        var transactions = new List<GroupTransaction>();
        while (await reader.ReadAsync())
        {
            transactions.Add(new GroupTransaction
            {
                Id          = reader.GetInt32(0),
                GroupId     = reader.GetInt32(1),
                Amount      = reader.GetDecimal(2),
                Description = reader.GetString(3),
                Date        = reader.GetDateTime(4),
                Type        = (TransactionType)reader.GetInt32(5),
                UserId      = reader.GetInt32(6)
            });
        }

        return transactions;

    }
         private async Task<List<GroupMember>> GetMembersForGroupAsync(int groupId)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT GroupId, UserId, JoinedAt
            FROM GroupMembers
            WHERE GroupId = @groupId";

        var param = command.CreateParameter();
        param.ParameterName = "@groupId";
        param.Value = groupId;
        command.Parameters.Add(param);

        using var reader = await command.ExecuteReaderAsync();

        var members = new List<GroupMember>();
        while (await reader.ReadAsync())
        {
            members.Add(new GroupMember
            {
                GroupId  = reader.GetInt32(0),
                UserId   = reader.GetInt32(1),
                JoinedAt = reader.GetDateTime(2)
            });
        }

        return members;
    }
    
}