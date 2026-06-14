using Spaartje.Domain.Models;

namespace Spaartje.BLL.Services;

public interface ICategoryService
{
    // Get all categories for a specific user.
    Task<List<Category>> GetCategoriesForUserAsync(int userId);

    // Get a single category — returns null if not found.
    Task<Category?> GetByIdAsync(int id);

    // Create a new category for a user.
    Task CreateCategoryAsync(string name, string description, int userId);

    // Delete a category. The userId is used to verify ownership.
    Task DeleteCategoryAsync(int categoryId, int userId);

    Task UpdateCategoryAsync(int categoryId, string name, string description, int userId);

     Task SetBudgetLimitAsync(int categoryId, decimal? budgetLimit, int userId);
}