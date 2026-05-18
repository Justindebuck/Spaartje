using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Spaartje.BLL.Services;
using Spaartje.DAL.Data;

namespace Spaartje.Web.Pages;


[Authorize]
public class DashboardModel : PageModel
{
   private readonly UserManager<IdentityUser> _userManager;

    public DashboardModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }
    public string UserEmail { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }
    public async Task OnGetAsync()
    {
      
        UserEmail = User.Identity?.Name ?? "Unknown";

         var user = await _userManager.GetUserAsync(User);

        if (user != null)
        {
            // IsInRoleAsync checks the AspNetUserRoles table.
            // Returns true if the user has the "Admin" role.
            IsAdmin = await _userManager.IsInRoleAsync(user, DbSeeder.AdminRole);
        }
    }
    
}