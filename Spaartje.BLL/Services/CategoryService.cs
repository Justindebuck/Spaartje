using Spaartje.DAL.Repositories;
using Spaartje.Domain.Models;

namespace Spaartje.BLL.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<Category>> GetCategoriesForUserAsync(int userId)
    {
        // Pass the userId to the repository so it filters by owner.
        return await _categoryRepository.GetCategoriesByUserIdAsync(userId);
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _categoryRepository.GetByIdAsync(id);
    }

    public async Task CreateCategoryAsync(string name, string description, int userId)
    {
        // Create the Category domain object here in the BLL.
        // The Web layer passes raw strings; the BLL assembles the object.
        var category = new Category
        {
            Name = name,
            Description = description,
            UserId = userId
        };

        await _categoryRepository.AddAsync(category);
    }

    public async Task DeleteCategoryAsync(int categoryId, int userId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);

        // only delete if the category exists AND
        // belongs to the user making the request.
        // This prevents user A from deleting user B's categories.
        if (category == null || category.UserId != userId)
            return;

        await _categoryRepository.DeleteAsync(category);
    }

    public async Task UpdateCategoryAsync(int categoryId, string name, string description, int userId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);

        // only update if the category exists AND
        // belongs to the user making the request.
        if (category == null || category.UserId != userId)
            return;

        // Update the properties of the existing category.
        category.Name = name;
        category.Description = description;

        await _categoryRepository.UpdateAsync(category);
    }
    public async Task SetBudgetLimitAsync(int categoryId, decimal? budgetLimit, int userId)
{
    var category = await _categoryRepository.GetByIdAsync(categoryId);

    // Ownership check — only the owner can set a budget limit
    if (category == null || category.UserId != userId)
        return;

    // Set the limit — null means no limit
    category.BudgetLimit = budgetLimit;

    await _categoryRepository.UpdateAsync(category);
}
}