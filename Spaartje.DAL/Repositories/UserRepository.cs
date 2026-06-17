using Spaartje.DAL.Data;
using Spaartje.Domain.Models;
using Microsoft.EntityFrameworkCore;
namespace Spaartje.DAL.Repositories;


public class UserRepository : IUserRepository
{
   
    private readonly ApplicationDbContext _context;


    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // GetAllUsersAsync fetches every user from the database
    // and maps them to our Domain User model.
    public async Task<List<User>> GetAllUsersAsync()
    {
     var connection = _context.Database.GetDbConnection();
      if (connection.State != System.Data.ConnectionState.Open)
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Email, Password, UserName, Role, CreatedAt FROM Users";

        using var reader = await command.ExecuteReaderAsync();

        var users = new List<User>();
        while (await reader.ReadAsync())
        {
            users.Add(new User
            {
                Id       = reader.GetInt32(0),
                Email    = reader.GetString(1),
                Password = reader.GetString(2),
                UserName = reader.GetString(3),
                Role     = reader.GetString(4),
                CreatedAt = reader.GetDateTime(5)
            });
        }

        return users;
    }

    // GetUserByEmailAsync finds a single user by email.
    public async Task<User?> GetUserByEmailAsync(string email)
    {
      var connection = _context.Database.GetDbConnection();
       if (connection.State != System.Data.ConnectionState.Open)
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Email, Password, UserName, Role, CreatedAt FROM Users WHERE Email = @email";

        var param = command.CreateParameter();
        param.ParameterName = "@email";
        param.Value = email;
        command.Parameters.Add(param);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new User
            {
                Id       = reader.GetInt32(0),
                Email    = reader.GetString(1),
                Password = reader.GetString(2),
                UserName = reader.GetString(3),
                Role     = reader.GetString(4),
                CreatedAt = reader.GetDateTime(5)
            };
        }

        return null;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
     var connection = _context.Database.GetDbConnection();
       if (connection.State != System.Data.ConnectionState.Open)
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Email, Password, UserName, Role, CreatedAt FROM Users WHERE Id = @id";

        var param = command.CreateParameter();
        param.ParameterName = "@id";
        param.Value = id;
        command.Parameters.Add(param);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new User
            {
                Id       = reader.GetInt32(0),
                Email    = reader.GetString(1),
                Password = reader.GetString(2),
                UserName = reader.GetString(3),
                Role     = reader.GetString(4),
                CreatedAt = reader.GetDateTime(5)
            };
        }

        return null;
    }

    public async Task AddUserAsync(User user)
    {
     var connection = _context.Database.GetDbConnection();
       if (connection.State != System.Data.ConnectionState.Open)
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Users (Email, Password, UserName, Role, CreatedAt)
            VALUES (@email, @password, @userName, @role, @createdAt)";

        void AddParam(string name, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            command.Parameters.Add(p);
        }

        AddParam("@email",     user.Email);
        AddParam("@password",  user.Password);
        AddParam("@userName",  user.UserName);
        AddParam("@role",      user.Role);
        AddParam("@createdAt", user.CreatedAt);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
     var connection = _context.Database.GetDbConnection();
       if (connection.State != System.Data.ConnectionState.Open)
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM Users WHERE Email = @email";

        var param = command.CreateParameter();
        param.ParameterName = "@email";
        param.Value = email;
        command.Parameters.Add(param);

        var count = await command.ExecuteScalarAsync();
        return Convert.ToInt32(count) > 0;
    }

public async Task DeleteUserAsync(int userId)
{
    var connection = _context.Database.GetDbConnection();
    if (connection.State != System.Data.ConnectionState.Open)
        await connection.OpenAsync();

    // Delete related data first
    using (var cmd = connection.CreateCommand())
    {
        cmd.CommandText = "DELETE FROM Transactions WHERE UserId = @id";
        var p = cmd.CreateParameter();
        p.ParameterName = "@id";
        p.Value = userId;
        cmd.Parameters.Add(p);
        await cmd.ExecuteNonQueryAsync();
    }

    using (var cmd = connection.CreateCommand())
    {
        cmd.CommandText = "DELETE FROM Categories WHERE UserId = @id";
        var p = cmd.CreateParameter();
        p.ParameterName = "@id";
        p.Value = userId;
        cmd.Parameters.Add(p);
        await cmd.ExecuteNonQueryAsync();
    }

    using (var cmd = connection.CreateCommand())
    {
        cmd.CommandText = "DELETE FROM Users WHERE Id = @id";
        var p = cmd.CreateParameter();
        p.ParameterName = "@id";
        p.Value = userId;
        cmd.Parameters.Add(p);
        await cmd.ExecuteNonQueryAsync();
    }
}
}