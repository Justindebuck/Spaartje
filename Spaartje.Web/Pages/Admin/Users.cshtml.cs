using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spaartje.BLL.Services;
using Spaartje.DAL.Data;
using Spaartje.Domain.Models;
using System.Security.Claims;

namespace Spaartje.Web.Pages.Admin;

[Authorize(Roles = DbSeeder.AdminRole)]
public class UsersModel : PageModel
{
   
    private readonly IUserService _userService;

    public UsersModel(IUserService userService)
    {
        _userService = userService;
    }

    public List<User> Users { get; set; } = new();

     public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostDeleteUserAsync(int userIdToDelete)
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim == null) return RedirectToPage("/Account/Login");
    var requestingUserId = int.Parse(userIdClaim);

    var error = await _userService.DeleteUserAsync(userIdToDelete, requestingUserId);
    if (error != null)
        ErrorMessage = error;

    return RedirectToPage();
}

    public async Task OnGetAsync()
    {
       try
        {
        Users = await _userService.GetAllUsersAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading users: {ex.Message}";
        }
        }

    
        
    
            
    }
