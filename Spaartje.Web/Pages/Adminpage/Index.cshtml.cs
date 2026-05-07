// Pages/Admin/Index.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace WEB.Pages.Admin;

// WHY both the policy attribute AND the folder convention in Program.cs?
// The folder convention is the primary guard. The [Authorize] attribute
// here is defensive documentation — it makes the protection visible
// to any developer reading this file, even without checking Program.cs.
[Authorize(Policy = "RequireAdmin")]
public class IndexModel : PageModel
{
    public void OnGet() { }
}