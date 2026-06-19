using Moq;
using Xunit;
using Spaartje.BLL.Services;
using Spaartje.DAL.Repositories;
using Spaartje.Domain.Models;

namespace Spaartje.Tests;

public class CategoryServiceTests
{
    // ─────────────────────────────────────────────
    // TEST 1: GetCategoriesForUser returns only that user's categories
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetCategoriesForUserAsync_ReturnsOnlyUsersCategories()
    {
        // Arrange
        var userId = 1;

        var fakeCategories = new List<Category>
        {
            new Category { Id = 1, Name = "Food",   UserId = userId },
            new Category { Id = 2, Name = "Salary", UserId = userId }
        };

        var mockRepo = new Mock<ICategoryRepository>();

        // The repository is called with the specific userId.
        // It returns only that user's categories.
        mockRepo.Setup(r => r.GetCategoriesByUserIdAsync(userId))
                .ReturnsAsync(fakeCategories);

        var service = new CategoryService(mockRepo.Object);

        // Act
        var result = await service.GetCategoriesForUserAsync(userId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(userId, c.UserId));
    }

    // ─────────────────────────────────────────────
    // TEST 2: CreateCategory saves with correct UserId
    // ─────────────────────────────────────────────

    [Fact]
    public async Task CreateCategoryAsync_SavesCategoryWithCorrectUserId()
    {
        // Arrange
        var userId = 1;

        // We need to capture what was passed to AddAsync
        // so we can check it was built correctly.
        Category? savedCategory = null;

        var mockRepo = new Mock<ICategoryRepository>();

        // Callback: "when AddAsync is called, capture the argument in savedCategory"
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Category>()))
                .Callback<Category>(c => savedCategory = c)
                .Returns(Task.CompletedTask);

        var service = new CategoryService(mockRepo.Object);

        // Act
        await service.CreateCategoryAsync("Food", "Groceries", userId);

        // Assert
        // Check that AddAsync was actually called (not skipped).
        Assert.NotNull(savedCategory);

        // Check the saved category has the correct values.
        Assert.Equal("Food",      savedCategory.Name);
        Assert.Equal("Groceries", savedCategory.Description);
        Assert.Equal(userId,      savedCategory.UserId);
    }

    // ─────────────────────────────────────────────
    // TEST 3: DeleteCategory — owner can delete their category
    // ─────────────────────────────────────────────

    [Fact]
    public async Task DeleteCategoryAsync_WhenOwner_DeletesCategory()
    {
        // Arrange
        var userId = 1;
        var category = new Category { Id = 1, Name = "Food", UserId = userId };

        var mockRepo = new Mock<ICategoryRepository>();

        mockRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(category);

        // DeleteAsync returns a completed Task (void equivalent).
        mockRepo.Setup(r => r.DeleteAsync(category))
                .Returns(Task.CompletedTask);

        var service = new CategoryService(mockRepo.Object);

        // Act
        await service.DeleteCategoryAsync(1, userId);

        // Assert
        // Verify that DeleteAsync was called exactly once with the correct category.
        // This proves the delete actually happened.
        mockRepo.Verify(r => r.DeleteAsync(category), Times.Once);
    }

    // ─────────────────────────────────────────────
    // TEST 4: DeleteCategory — non-owner CANNOT delete
    // This tests the business rule: only the owner can delete
    // ─────────────────────────────────────────────

    [Fact]
    public async Task DeleteCategoryAsync_WhenNotOwner_DoesNotDelete()
    {
        // Arrange
        var ownerId   = 1;
        var attackerId = 20; // A different user trying to delete

        var category = new Category { Id = 1, Name = "Food", UserId = ownerId };

        var mockRepo = new Mock<ICategoryRepository>();

        mockRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(category);

        var service = new CategoryService(mockRepo.Object);

        // Act — attacker tries to delete owner's category
        await service.DeleteCategoryAsync(1, attackerId);

        // Assert
        // Verify that DeleteAsync was NEVER called.
        // Times.Never means "this should not have been called at all".
        mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Category>()), Times.Never);
    }

    // ─────────────────────────────────────────────
    // TEST 5: DeleteCategory — category does not exist
    // ─────────────────────────────────────────────

    [Fact]
    public async Task DeleteCategoryAsync_WhenCategoryNotFound_DoesNotDelete()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();

        // Simulate: category not found in database
        mockRepo.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Category?)null);

        var service = new CategoryService(mockRepo.Object);

        // Act
        await service.DeleteCategoryAsync(999, 1);

        // Assert
        // Delete should never be called if the category doesn't exist.
        mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Category>()), Times.Never);
    }
}