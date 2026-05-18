using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spaartje.BLL.Services;
using Spaartje.DAL.Data;
using Spaartje.Domain.Models;

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
