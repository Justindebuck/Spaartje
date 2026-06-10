using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Spaartje.BLL.Services;

namespace Spaartje.Web.Pages.Groups;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IGroupService _groupService;
    private readonly UserManager<IdentityUser> _userManager;

    public CreateModel(IGroupService groupService, UserManager<IdentityUser> userManager)
    {
        _groupService = groupService;
        _userManager = userManager;
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

        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToPage("/Login");

        var group = await _groupService.CreateGroupAsync(
            Input.Name,
            Input.BudgetLimit,
            userId);

        return RedirectToPage("/Groups/Details", new { id = group.Id });
    }
}