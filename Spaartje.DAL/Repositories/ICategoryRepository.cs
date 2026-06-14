using Spaartje.Domain.Models;

namespace Spaartje.DAL.Repositories;


public interface ICategoryRepository
{
    // Get all categories that belong to a specific user.
    // userId is the Identity user's GUID string.
    Task<List<Category>> GetCategoriesByUserIdAsync(int userId);

    // Get a single category by its ID.
    // Returns null if not found.
    Task<Category?> GetByIdAsync(int id);

    // Save a new category to the database.
    Task AddAsync(Category category);

    // Delete a category from the database.
    Task DeleteAsync(Category category);
    
    Task UpdateAsync(Category category);
}