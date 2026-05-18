using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spaartje.DAL.Data;

namespace Spaartje.Web.Pages.Admin;

[Authorize(Roles = DbSeeder.AdminRole)]
public class IndexModel : PageModel
{
    public void OnGet()
    {
       
    }
}