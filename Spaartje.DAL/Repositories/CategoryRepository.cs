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

    public async Task<List<Category>> GetCategoriesByUserIdAsync(string userId)
    {
        // _context.Categories queries the Categories table.
        // .Where() filters rows — like a SQL WHERE clause.
        // .OrderBy() sorts the results — like SQL ORDER BY.
        // .ToListAsync() executes the query and returns the results.
        // The generated SQL looks like:
        // SELECT * FROM Categories WHERE UserId = @userId ORDER BY Name
        return await _context.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        // FindAsync looks up a row by its Primary Key (Id).
        // It returns null if no row with that Id exists.
        // Generated SQL: SELECT * FROM Categories WHERE Id = @id LIMIT 1
        return await _context.Categories.FindAsync(id);
    }

    public async Task AddAsync(Category category)
    {
        // AddAsync stages the new category for insertion.
        // Nothing is written to the database yet.
        await _context.Categories.AddAsync(category);

        // SaveChangesAsync executes the staged INSERT statement.
        // Generated SQL: INSERT INTO Categories (Name, Description, UserId)
        //                VALUES (@name, @description, @userId)
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        // Remove stages the category for deletion.
        _context.Categories.Remove(category);

        // SaveChangesAsync executes the DELETE statement.
        // Generated SQL: DELETE FROM Categories WHERE Id = @id
        await _context.SaveChangesAsync();
    }


    public async Task UpdateAsync(Category category)
    {
        // Update stages the category for update.
        _context.Categories.Update(category);

        // SaveChangesAsync executes the UPDATE statement.
        // Generated SQL: UPDATE Categories SET Name = @name, Description = @description
        await _context.SaveChangesAsync();
    }
}