using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Spaartje.BLL.Services;

namespace Spaartje.Web.Pages.Groups;

[Authorize]
public class EditModel : PageModel
{
    private readonly IGroupService _groupService;

    public EditModel(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Group name is required")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // No [Required] — leaving it empty means no budget limit
    [BindProperty]
    [Range(0, double.MaxValue, ErrorMessage = "Budget must be 0 or more")]
    public decimal? BudgetLimit { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return RedirectToPage("/Account/Login");
        var userId = int.Parse(userIdClaim);

        var group = await _groupService.GetGroupByIdAsync(Id, userId);
        if (group == null) return RedirectToPage("/Groups/Index");

        // Only the manager can access this page
        if (group.OwnerId != userId)
            return RedirectToPage("/Groups/Details", new { id = Id });

        Name        = group.Name;
        BudgetLimit = group.BudgetLimit;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return RedirectToPage("/Account/Login");
        var userId = int.Parse(userIdClaim);

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please check the form.";
            return Page();
        }

        var error = await _groupService.UpdateGroupAsync(Id, Name, BudgetLimit, userId);
        if (error != null)
        {
            ErrorMessage = error;
            return Page();
        }

        return RedirectToPage("/Groups/Details", new { id = Id });
    }
}