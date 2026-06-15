using Microsoft.EntityFrameworkCore;
using Spaartje.DAL.Data;
using Spaartje.Domain.Models;

namespace Spaartje.DAL.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Transaction>> GetTransactionsByUserIdAsync(int userId)
    {
       var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT t.Id, t.Amount, t.Description, t.Date, t.Type, t.CategoryId, t.UserId,
                   c.Id, c.Name, c.Description, c.UserId, c.BudgetLimit
            FROM Transactions t
            LEFT JOIN Categories c ON c.Id = t.CategoryId
            WHERE t.UserId = @userId
            ORDER BY t.Date DESC";

        var param = command.CreateParameter();
        param.ParameterName = "@userId";
        param.Value = userId;
        command.Parameters.Add(param);

        using var reader = await command.ExecuteReaderAsync();

        var transactions = new List<Transaction>();
        while (await reader.ReadAsync())
        {
            transactions.Add(MapTransaction(reader));
        }

        return transactions;
    }

    public async Task<List<Transaction>> GetTransactionsByUserIdAndCategoryAsync(int userId, int categoryId)
    {
       var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT t.Id, t.Amount, t.Description, t.Date, t.Type, t.CategoryId, t.UserId,
                   c.Id, c.Name, c.Description, c.UserId, c.BudgetLimit
            FROM Transactions t
            LEFT JOIN Categories c ON c.Id = t.CategoryId
            WHERE t.UserId = @userId AND t.CategoryId = @categoryId
            ORDER BY t.Date DESC";

        void AddParam(string name, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            command.Parameters.Add(p);
        }

        AddParam("@userId",     userId);
        AddParam("@categoryId", categoryId);

        using var reader = await command.ExecuteReaderAsync();

        var transactions = new List<Transaction>();
        while (await reader.ReadAsync())
        {
            transactions.Add(MapTransaction(reader));
        }

        return transactions;
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
       var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT t.Id, t.Amount, t.Description, t.Date, t.Type, t.CategoryId, t.UserId,
                   c.Id, c.Name, c.Description, c.UserId, c.BudgetLimit
            FROM Transactions t
            LEFT JOIN Categories c ON c.Id = t.CategoryId
            WHERE t.Id = @id";

        var param = command.CreateParameter();
        param.ParameterName = "@id";
        param.Value = id;
        command.Parameters.Add(param);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
            return MapTransaction(reader);

        return null;
    }

    public async Task AddAsync(Transaction transaction)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Transactions (Amount, Description, Date, Type, CategoryId, UserId)
            VALUES (@amount, @description, @date, @type, @categoryId, @userId)";

        void AddParam(string name, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            command.Parameters.Add(p);
        }

        AddParam("@amount",      transaction.Amount);
        AddParam("@description", transaction.Description);
        AddParam("@date",        transaction.Date);
        AddParam("@type",        (int)transaction.Type);
        AddParam("@categoryId",  transaction.CategoryId);
        AddParam("@userId",      transaction.UserId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Transaction transaction)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Transactions WHERE Id = @id";

        var param = command.CreateParameter();
        param.ParameterName = "@id";
        param.Value = transaction.Id;
        command.Parameters.Add(param);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Transaction transaction)
    {
         var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Transactions
            SET Amount      = @amount,
                Description = @description,
                Date        = @date,
                Type        = @type,
                CategoryId  = @categoryId
            WHERE Id = @id";

        void AddParam(string name, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            command.Parameters.Add(p);
        }

        AddParam("@amount",      transaction.Amount);
        AddParam("@description", transaction.Description);
        AddParam("@date",        transaction.Date);
        AddParam("@type",        (int)transaction.Type);
        AddParam("@categoryId",  transaction.CategoryId);
        AddParam("@id",          transaction.Id);

        await command.ExecuteNonQueryAsync();
    }
     private static Transaction MapTransaction(System.Data.Common.DbDataReader reader)
    {
        var transaction = new Transaction
        {
            Id          = reader.GetInt32(0),
            Amount      = reader.GetDecimal(1),
            Description = reader.GetString(2),
            Date        = reader.GetDateTime(3),
            Type        = (TransactionType)reader.GetInt32(4),
            CategoryId  = reader.GetInt32(5),
            UserId      = reader.GetInt32(6)
        };

        // Only map the category if the JOIN returned a category row
        if (!reader.IsDBNull(7))
        {
            transaction.Category = new Category
            {
                Id          = reader.GetInt32(7),
                Name        = reader.GetString(8),
                Description = reader.GetString(9),
                UserId      = reader.GetInt32(10),
                BudgetLimit = reader.IsDBNull(11) ? null : reader.GetDecimal(11)
            };
        }

        return transaction;
    }
}