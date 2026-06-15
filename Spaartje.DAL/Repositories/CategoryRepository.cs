using Microsoft.EntityFrameworkCore;
using Spaartje.DAL.Data;
using Spaartje.Domain.Models;

namespace Spaartje.DAL.Repositories;

public class CategoryRepository : ICategoryRepository
{
    // We inject ApplicationDbContext directly here in the DAL.
    // The DAL is the ONLY layer that ever touches DbContext.
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetCategoriesByUserIdAsync(int userId)
    {
       var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, UserId, BudgetLimit
            FROM Categories
            WHERE UserId = @userId
            ORDER BY Name";

        var param = command.CreateParameter();
        param.ParameterName = "@userId";
        param.Value = userId;
        command.Parameters.Add(param);

        using var reader = await command.ExecuteReaderAsync();

        var categories = new List<Category>();
        while (await reader.ReadAsync())
        {
            categories.Add(new Category
            {
                Id          = reader.GetInt32(0),
                Name        = reader.GetString(1),
                Description = reader.GetString(2),
                UserId      = reader.GetInt32(3),
                BudgetLimit = reader.IsDBNull(4) ? null : reader.GetDecimal(4)
            });
        }

        return categories;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
         var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, UserId, BudgetLimit
            FROM Categories
            WHERE Id = @id";

        var param = command.CreateParameter();
        param.ParameterName = "@id";
        param.Value = id;
        command.Parameters.Add(param);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Category
            {
                Id          = reader.GetInt32(0),
                Name        = reader.GetString(1),
                Description = reader.GetString(2),
                UserId      = reader.GetInt32(3),
                BudgetLimit = reader.IsDBNull(4) ? null : reader.GetDecimal(4)
            };
        }

        return null;
    }

    public async Task AddAsync(Category category)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Categories (Name, Description, UserId, BudgetLimit)
            VALUES (@name, @description, @userId, @budgetLimit)";

        void AddParam(string name, object? value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            command.Parameters.Add(p);
        }

        AddParam("@name",        category.Name);
        AddParam("@description", category.Description);
        AddParam("@userId",      category.UserId);
        AddParam("@budgetLimit", category.BudgetLimit);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Category category)
    {
       var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Categories WHERE Id = @id";

        var param = command.CreateParameter();
        param.ParameterName = "@id";
        param.Value = category.Id;
        command.Parameters.Add(param);

        await command.ExecuteNonQueryAsync();
    }


    public async Task UpdateAsync(Category category)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Categories
            SET Name        = @name,
                Description = @description,
                BudgetLimit = @budgetLimit
            WHERE Id = @id";

        void AddParam(string name, object? value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            command.Parameters.Add(p);
        }

        AddParam("@name",        category.Name);
        AddParam("@description", category.Description);
        AddParam("@budgetLimit", category.BudgetLimit);
        AddParam("@id",          category.Id);

        await command.ExecuteNonQueryAsync();
    }
}