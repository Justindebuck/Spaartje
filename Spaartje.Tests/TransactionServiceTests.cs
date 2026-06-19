using Moq;
using Xunit;
using Spaartje.BLL.Services;
using Spaartje.DAL.Repositories;
using Spaartje.Domain.Models;

namespace Spaartje.Tests;

public class TransactionServiceTests
{
    // ─────────────────────────────────────────────
    // TEST 1: GetTransactionsForUser returns correct transactions
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetTransactionsForUserAsync_ReturnsUsersTransactions()
    {
        // Arrange
        var userId = 1;

        var fakeTransactions = new List<Transaction>
        {
            new Transaction { Id = 1, Amount = 45.00m,   Type = TransactionType.Expense, UserId = userId },
            new Transaction { Id = 2, Amount = 1200.00m, Type = TransactionType.Income,  UserId = userId }
        };

        var mockRepo = new Mock<ITransactionRepository>();
        mockRepo.Setup(r => r.GetTransactionsByUserIdAsync(userId))
                .ReturnsAsync(fakeTransactions);

        var service = new TransactionService(mockRepo.Object);

        // Act
        var result = await service.GetTransactionsForUserAsync(userId);

        // Assert
        Assert.Equal(2, result.Count);
    }

    // ─────────────────────────────────────────────
    // TEST 2: CreateTransaction with valid data saves correctly
    // ─────────────────────────────────────────────

    [Fact]
    public async Task CreateTransactionAsync_WithValidData_SavesTransaction()
    {
        // Arrange
        var userId = 1;
        Transaction? saved = null;

        var mockRepo = new Mock<ITransactionRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
                .Callback<Transaction>(t => saved = t)
                .Returns(Task.CompletedTask);

        var service = new TransactionService(mockRepo.Object);

        // Act
        await service.CreateTransactionAsync(
            amount: 45.00m,
            description: "Groceries",
            date: new DateTime(2026, 5, 1),
            type: TransactionType.Expense,
            categoryId: 1,
            userId: userId);

        // Assert
        Assert.NotNull(saved);
        Assert.Equal(45.00m,                 saved.Amount);
        Assert.Equal("Groceries",            saved.Description);
        Assert.Equal(TransactionType.Expense, saved.Type);
        Assert.Equal(userId,                 saved.UserId);
        Assert.Equal(1,                      saved.CategoryId);
    }

    // ─────────────────────────────────────────────
    // TEST 3: CreateTransaction with zero amount throws exception
    // This tests the business rule: amount must be > 0
    // ─────────────────────────────────────────────

    [Fact]
    public async Task CreateTransactionAsync_WithZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var mockRepo = new Mock<ITransactionRepository>();
        var service = new TransactionService(mockRepo.Object);

        // Act & Assert combined:
        // Assert.ThrowsAsync checks that calling the method throws the expected exception.
        // If it does NOT throw, the test fails.
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateTransactionAsync(
                amount: 0m,        // ← zero amount should be rejected
                description: "Test",
                date: DateTime.Today,
                type: TransactionType.Expense,
                categoryId: 1,
                userId: 1));
    }

    // ─────────────────────────────────────────────
    // TEST 4: CreateTransaction with negative amount throws exception
    // ─────────────────────────────────────────────

    [Fact]
    public async Task CreateTransactionAsync_WithNegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var mockRepo = new Mock<ITransactionRepository>();
        var service = new TransactionService(mockRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateTransactionAsync(
                amount: -10m,     // ← negative amount should be rejected
                description: "Test",
                date: DateTime.Today,
                type: TransactionType.Expense,
                categoryId: 1,
                userId: 1));
    }

    // ─────────────────────────────────────────────
    // TEST 5: DeleteTransaction — owner can delete
    // ─────────────────────────────────────────────

    [Fact]
    public async Task DeleteTransactionAsync_WhenOwner_DeletesTransaction()
    {
        // Arrange
        var userId = 1;
        var transaction = new Transaction { Id = 1, Amount = 50m, UserId = userId };

        var mockRepo = new Mock<ITransactionRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(transaction);
        mockRepo.Setup(r => r.DeleteAsync(transaction)).Returns(Task.CompletedTask);

        var service = new TransactionService(mockRepo.Object);

        // Act
        await service.DeleteTransactionAsync(1, userId);

        // Assert
        mockRepo.Verify(r => r.DeleteAsync(transaction), Times.Once);
    }

    // ─────────────────────────────────────────────
    // TEST 6: DeleteTransaction — non-owner CANNOT delete
    // ─────────────────────────────────────────────

    [Fact]
    public async Task DeleteTransactionAsync_WhenNotOwner_DoesNotDelete()
    {
        // Arrange
        var ownerId    = 1;
        var attackerId = 20;

        var transaction = new Transaction { Id = 1, Amount = 50m, UserId = ownerId };

        var mockRepo = new Mock<ITransactionRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(transaction);

        var service = new TransactionService(mockRepo.Object);

        // Act
        await service.DeleteTransactionAsync(1, attackerId);

        // Assert — delete should NEVER have been called
        mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Transaction>()), Times.Never);
    }

    // ─────────────────────────────────────────────
    // TEST 7: [Theory] — Test multiple amounts at once
    // A Theory runs the same test with different inputs.
    // This saves writing the same test multiple times.
    // ─────────────────────────────────────────────

    [Theory]
    [InlineData(0)]       // zero
    [InlineData(-1)]      // negative
    [InlineData(-100)]    // large negative
    public async Task CreateTransactionAsync_WithInvalidAmount_ThrowsArgumentException(
        decimal invalidAmount)
    {
        // Arrange
        var mockRepo = new Mock<ITransactionRepository>();
        var service = new TransactionService(mockRepo.Object);

        // Act & Assert
        // This runs three times — once for each [InlineData] value
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateTransactionAsync(
                amount: invalidAmount,
                description: "Test",
                date: DateTime.Today,
                type: TransactionType.Expense,
                categoryId: 1,
                userId: 1));
    }
}