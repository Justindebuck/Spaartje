using Spaartje.DAL.Repositories;
using Spaartje.Domain.Models;

namespace Spaartje.BLL.Services;

public class DashboardService : IDashboardService
{
    private readonly ITransactionRepository _transactionRepository;

    private readonly ICategoryRepository _categoryRepository;   

    public DashboardService(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<DashboardSummary> GetSummaryForUserAsync(string userId)
    {
        // Load all transactions for this user (with their categories included).
        var transactions = await _transactionRepository.GetTransactionsByUserIdAsync(userId);

        var summary = new DashboardSummary();

        // LINQ Sum() adds up all the Amount values where the condition is true.
        // This is equivalent to: loop through all transactions, add up the amounts
        // where Type == Income. LINQ makes this a single readable line.
        summary.TotalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        summary.TotalExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        // GroupBy groups transactions by their category name.
        // This is like SQL: GROUP BY Category.Name
        // For each group, we calculate the totals.
        summary.CategoryBreakdown = transactions
            .GroupBy(t => t.Category?.Name ?? "Uncategorised")
            .Select(group => new CategorySummary
            {
                CategoryName = group.Key,
                TotalIncome = group
                    .Where(t => t.Type == TransactionType.Income)
                    .Sum(t => t.Amount),
                TotalExpenses = group
                    .Where(t => t.Type == TransactionType.Expense)
                    .Sum(t => t.Amount),
                TransactionCount = group.Count()
            })
            .OrderByDescending(c => c.TotalExpenses)
            .ToList();

        return summary;
    }
    public async Task<List<BudgetSummary>> GetBudgetSummaryAsync(string userId)
{
    // Get all categories for this user
    var categories = await _categoryRepository.GetCategoriesByUserIdAsync(userId);

    // Only care about categories that have a budget limit set
    var categoriesWithBudget = categories.Where(c => c.BudgetLimit.HasValue && c.BudgetLimit > 0).ToList();

    if (!categoriesWithBudget.Any())
        return new List<BudgetSummary>();

    // Get all transactions for this user
    var allTransactions = await _transactionRepository.GetTransactionsByUserIdAsync(userId);

    // Filter to only this month's expense transactions
    var now = DateTime.Now;
    var thisMonthExpenses = allTransactions
        .Where(t => t.Type == TransactionType.Expense
                 && t.Date.Year == now.Year
                 && t.Date.Month == now.Month)
        .ToList();

    // Build a BudgetSummary for each category that has a limit
    var budgetSummaries = categoriesWithBudget.Select(category =>
    {
        // Sum up this month's expenses for this specific category
        var spentThisMonth = thisMonthExpenses
            .Where(t => t.CategoryId == category.Id)
            .Sum(t => t.Amount);

        return new BudgetSummary
        {
            CategoryName    = category.Name,
            BudgetLimit     = category.BudgetLimit!.Value,
            SpentThisMonth  = spentThisMonth
        };
    }).ToList();

    return budgetSummaries;
}
}