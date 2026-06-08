using Spaartje.Domain.Models;

namespace Spaartje.BLL.Services;

// DashboardSummary is a simple data container (a DTO — Data Transfer Object).
// It holds the calculated totals that the dashboard page needs to display.
// It lives here in the BLL because it is produced by business logic calculations.
public class DashboardSummary
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }

    // Balance is a calculated property — not stored in the database.
    // It is always TotalIncome minus TotalExpenses.
    // The 'get' only accessor means it cannot be set directly — it is always calculated.
    public decimal Balance => TotalIncome - TotalExpenses;

    // A breakdown of expenses per category.
    public List<CategorySummary> CategoryBreakdown { get; set; } = new();
}

// CategorySummary holds the totals for one category.
public class CategorySummary
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalExpenses { get; set; }
    public decimal TotalIncome { get; set; }
    public int TransactionCount { get; set; }
}

public interface IDashboardService
{
    Task<DashboardSummary> GetSummaryForUserAsync(string userId);
    Task<List<BudgetSummary>> GetBudgetSummaryAsync(string userId);
}