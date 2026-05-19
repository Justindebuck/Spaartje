using Spaartje.DAL.Repositories;
using Spaartje.Domain.Models;

namespace Spaartje.BLL.Services;

public class DashboardService : IDashboardService
{
    private readonly ITransactionRepository _transactionRepository;

    public DashboardService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
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
}