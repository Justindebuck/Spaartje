using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Spaartje.BLL.Services;
using System.Security.Claims;

namespace Spaartje.Web.Pages.Groups;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IGroupService _groupService;

    public CreateModel(IGroupService groupService)
    {
        _groupService = groupService;
        
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Group name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Budget limit must be greater than zero")]
        public decimal? BudgetLimit { get; set; }
    }

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return RedirectToPage("/Login");

        var userId = int.Parse(userIdClaim);

        var group = await _groupService.CreateGroupAsync(
            Input.Name,
            Input.BudgetLimit,
            userId);

        return RedirectToPage("/Groups/Details", new { id = group.Id });
    }
}