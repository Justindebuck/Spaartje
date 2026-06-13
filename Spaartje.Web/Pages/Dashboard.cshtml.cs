using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Spaartje.BLL.Services;

using System.Security.Claims;


namespace Spaartje.Web.Pages;


[Authorize]
public class DashboardModel : PageModel
{

    private readonly IDashboardService _dashboardService;
    public DashboardModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }
    public string UserEmail { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }
    public DashboardSummary? Summary { get; set; }
      public List<BudgetSummary> BudgetSummaries { get; set; } = new();   

    public async Task OnGetAsync()
    {
      
        UserEmail = User.Identity?.Name ?? "Unknown";
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return;

        var userId = int.Parse(userIdClaim);

        IsAdmin = User.IsInRole("Admin");
        Summary = await _dashboardService.GetSummaryForUserAsync(userId);

        BudgetSummaries = await _dashboardService.GetBudgetSummaryAsync(userId);
        }
    }
    