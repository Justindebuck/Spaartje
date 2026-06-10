using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spaartje.BLL.Services;
using Spaartje.Domain.Models;

namespace Spaartje.Web.Pages.Groups;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IGroupService _groupService;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(IGroupService groupService, UserManager<IdentityUser> userManager)
    {
        _groupService = groupService;
        _userManager = userManager;
    }

    public List<Group> Groups { get; set; } = new();
    public string CurrentUserId { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return;

        CurrentUserId = userId;
        Groups = await _groupService.GetGroupsForUserAsync(userId);
    }
}