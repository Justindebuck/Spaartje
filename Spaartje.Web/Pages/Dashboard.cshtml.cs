using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Spaartje.Web.Pages;


[Authorize]
public class DashboardModel : PageModel
{
   
    public string UserEmail { get; set; } = string.Empty;

  
    public void OnGet()
    {
      
        UserEmail = User.Identity?.Name ?? "Unknown";
    }
}