using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spaartje.BLL.Services;
using Spaartje.Domain.Models;

using System.Security.Claims;

namespace Spaartje.Web.Pages.Groups;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IGroupService _groupService;


    public IndexModel(IGroupService groupService)
    {
        _groupService = groupService;
       
    }

    public List<Group> Groups { get; set; } = new();
    public int CurrentUserId { get; set; } 

    public async Task OnGetAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return;

        var userId = int.Parse(userIdClaim);
        CurrentUserId = userId;
        Groups = await _groupService.GetGroupsForUserAsync(userId);
    }
}