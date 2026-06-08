namespace Spaartje.BLL.Services;

public class BudgetSummary
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal BudgetLimit { get; set; }
    public decimal SpentThisMonth { get; set; }
    public decimal Remaining => BudgetLimit - SpentThisMonth;
    public decimal PercentageUsed => BudgetLimit > 0
        ? Math.Round((SpentThisMonth / BudgetLimit) * 100, 1)
        : 0;
    public bool IsOverBudget => SpentThisMonth > BudgetLimit;
}