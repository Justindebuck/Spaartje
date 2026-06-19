using Moq;
using Xunit;
using Spaartje.BLL.Services;
using Spaartje.DAL.Repositories;
using Spaartje.Domain.Models;

namespace Spaartje.Tests;

public class DashboardServiceTests
{
    // Helper: creates a transaction quickly without repeating all properties
    private static Transaction MakeTransaction(
        decimal amount, TransactionType type, string categoryName = "Food", int userId = 1)
    {
        return new Transaction
        {
            Amount = amount,
            Type = type,
            UserId = userId,
            Category = new Category { Name = categoryName }
        };
    }

    // ─────────────────────────────────────────────
    // TEST 1: Total income is calculated correctly
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetSummaryForUserAsync_CalculatesTotalIncomeCorrectly()
    {
        // Arrange
        var userId = 1;

        var transactions = new List<Transaction>
        {
            MakeTransaction(1000m, TransactionType.Income),
            MakeTransaction(200m,  TransactionType.Income),
            MakeTransaction(50m,   TransactionType.Expense) // should NOT count toward income
        };

        var mockTransactionRepo = new Mock<ITransactionRepository>();
        mockTransactionRepo.Setup(r => r.GetTransactionsByUserIdAsync(userId))
                .ReturnsAsync(transactions);

        var mockCategoryRepo = new Mock<ICategoryRepository>();
        var service = new DashboardService(mockTransactionRepo.Object, mockCategoryRepo.Object);

        // Act
        var summary = await service.GetSummaryForUserAsync(userId);

        // Assert
        // 1000 + 200 = 1200 (the expense of 50 is excluded)
        Assert.Equal(1200m, summary.TotalIncome);
    }

    // ─────────────────────────────────────────────
    // TEST 2: Total expenses is calculated correctly
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetSummaryForUserAsync_CalculatesTotalExpensesCorrectly()
    {
        // Arrange
        var userId = 1;

        var transactions = new List<Transaction>
        {
            MakeTransaction(45m,   TransactionType.Expense),
            MakeTransaction(650m,  TransactionType.Expense),
            MakeTransaction(1200m, TransactionType.Income) // should NOT count toward expenses
        };

        var mockTransactionRepo = new Mock<ITransactionRepository>();
        mockTransactionRepo.Setup(r => r.GetTransactionsByUserIdAsync(userId))
                .ReturnsAsync(transactions);

        var mockCategoryRepo = new Mock<ICategoryRepository>();
        var service = new DashboardService(mockTransactionRepo.Object, mockCategoryRepo.Object);

        // Act
        var summary = await service.GetSummaryForUserAsync(userId);

        // Assert
        // 45 + 650 = 695
        Assert.Equal(695m, summary.TotalExpenses);
    }

    // ─────────────────────────────────────────────
    // TEST 3: Balance = Income - Expenses
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetSummaryForUserAsync_CalculatesBalanceCorrectly()
    {
        // Arrange
        var userId = 1;

        var transactions = new List<Transaction>
        {
            MakeTransaction(1200m, TransactionType.Income),
            MakeTransaction(695m,  TransactionType.Expense)
        };

        var mockTransactionRepo = new Mock<ITransactionRepository>();
        mockTransactionRepo.Setup(r => r.GetTransactionsByUserIdAsync(userId))
                .ReturnsAsync(transactions);

        var mockCategoryRepo = new Mock<ICategoryRepository>();
        var service = new DashboardService(mockTransactionRepo.Object, mockCategoryRepo.Object);

        // Act
        var summary = await service.GetSummaryForUserAsync(userId);

        // Assert
        // Balance is a calculated property: Income - Expenses = 1200 - 695 = 505
        Assert.Equal(505m, summary.Balance);
    }

    // ─────────────────────────────────────────────
    // TEST 4: Negative balance when expenses exceed income
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetSummaryForUserAsync_WhenExpensesExceedIncome_BalanceIsNegative()
    {
        // Arrange
        var userId = 1;

        var transactions = new List<Transaction>
        {
            MakeTransaction(500m,  TransactionType.Income),
            MakeTransaction(800m,  TransactionType.Expense)
        };

        var mockTransactionRepo = new Mock<ITransactionRepository>();
        mockTransactionRepo.Setup(r => r.GetTransactionsByUserIdAsync(userId))
                .ReturnsAsync(transactions);

        var mockCategoryRepo = new Mock<ICategoryRepository>();
        var service = new DashboardService(mockTransactionRepo.Object, mockCategoryRepo.Object);

        // Act
        var summary = await service.GetSummaryForUserAsync(userId);

        // Assert
        // 500 - 800 = -300 (negative balance)
        Assert.Equal(-300m, summary.Balance);

        // Also verify the balance is actually negative
        Assert.True(summary.Balance < 0);
    }

    // ─────────────────────────────────────────────
    // TEST 5: Empty dashboard (no transactions)
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetSummaryForUserAsync_WithNoTransactions_ReturnsZeroTotals()
    {
        // Arrange
        var userId = 1;

        var mockTransactionRepo = new Mock<ITransactionRepository>();
        mockTransactionRepo.Setup(r => r.GetTransactionsByUserIdAsync(userId))
                .ReturnsAsync(new List<Transaction>());

        var mockCategoryRepo = new Mock<ICategoryRepository>();
        var service = new DashboardService(mockTransactionRepo.Object, mockCategoryRepo.Object);

        // Act
        var summary = await service.GetSummaryForUserAsync(userId);

        // Assert
        Assert.Equal(0m, summary.TotalIncome);
        Assert.Equal(0m, summary.TotalExpenses);
        Assert.Equal(0m, summary.Balance);
        Assert.Empty(summary.CategoryBreakdown);
    }

    // ─────────────────────────────────────────────
    // TEST 6: Category breakdown groups correctly
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetSummaryForUserAsync_GroupsTransactionsByCategory()
    {
        // Arrange
        var userId = 1;

        var transactions = new List<Transaction>
        {
            MakeTransaction(30m,  TransactionType.Expense, "Food"),
            MakeTransaction(15m,  TransactionType.Expense, "Food"),   // same category
            MakeTransaction(650m, TransactionType.Expense, "Rent"),   // different category
            MakeTransaction(1200m, TransactionType.Income, "Salary")  // income category
        };

        var mockTransactionRepo = new Mock<ITransactionRepository>();
        mockTransactionRepo.Setup(r => r.GetTransactionsByUserIdAsync(userId))
                           .ReturnsAsync(transactions);

        var mockCategoryRepo = new Mock<ICategoryRepository>();
        var service = new DashboardService(mockTransactionRepo.Object, mockCategoryRepo.Object);

        // Act
        var summary = await service.GetSummaryForUserAsync(userId);

        // Assert — 3 categories: Food, Rent, Salary
        Assert.Equal(3, summary.CategoryBreakdown.Count);

        // Find the Food category in the breakdown
        var food = summary.CategoryBreakdown.FirstOrDefault(c => c.CategoryName == "Food");
        Assert.NotNull(food);
        // 30 + 15 = 45 total expenses for Food
        Assert.Equal(45m, food.TotalExpenses);
        Assert.Equal(2,   food.TransactionCount);

        // Find the Rent category
        var rent = summary.CategoryBreakdown.FirstOrDefault(c => c.CategoryName == "Rent");
        Assert.NotNull(rent);
        Assert.Equal(650m, rent.TotalExpenses);
        Assert.Equal(1,    rent.TransactionCount);
    }

    // ─────────────────────────────────────────────
    // TEST 7: [Theory] — Multiple income/expense combinations
    // ─────────────────────────────────────────────

    [Theory]
    [InlineData(1000, 500,  500)]   // income, expenses, expected balance
    [InlineData(2000, 2000, 0)]     // break even
    [InlineData(500,  800,  -300)]  // negative balance
    [InlineData(0,    0,    0)]     // all zero
    public async Task GetSummaryForUserAsync_BalanceCalculation_IsAlwaysCorrect(
        decimal income, decimal expenses, decimal expectedBalance)
    {
        // Arrange
        var userId = 1;
        var transactions = new List<Transaction>();

        if (income > 0)
            transactions.Add(MakeTransaction(income, TransactionType.Income));
        if (expenses > 0)
            transactions.Add(MakeTransaction(expenses, TransactionType.Expense));

        var mockTransactionRepo = new Mock<ITransactionRepository>();
        mockTransactionRepo.Setup(r => r.GetTransactionsByUserIdAsync(userId))
                            .ReturnsAsync(transactions);

        var mockCategoryRepo = new Mock<ICategoryRepository>();
        var service = new DashboardService(mockTransactionRepo.Object, mockCategoryRepo.Object);

        // Act
        var summary = await service.GetSummaryForUserAsync(userId);

        // Assert
        Assert.Equal(expectedBalance, summary.Balance);
    }
}