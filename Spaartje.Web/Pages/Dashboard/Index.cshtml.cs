// Pages/Dashboard/Index.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WEB.Pages.Dashboard;

[Authorize(Policy = "RequireUser")]
public class IndexModel : PageModel
{
    public string WelcomeMessage { get; private set; } = string.Empty;

    public void OnGet()
    {
        WelcomeMessage = $"Welcome back, {User.Identity?.Name}!";
    }
}