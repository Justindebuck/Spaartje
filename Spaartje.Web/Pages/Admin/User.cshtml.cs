using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spaartje.Web.Data;

namespace Spaartje.Web.Pages.Admin;

[Authorize(Roles = DbSeeder.AdminRole)]
public class UsersModel : PageModel
{
   
    private readonly UserManager<IdentityUser> _userManager;

    public UsersModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public List<UserViewModel> Users { get; set; } = new();

    public class UserViewModel
    {
        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
    
        public List<string> Roles { get; set; } = new();
    }

    public async Task OnGetAsync()
    {
       
        var allUsers = _userManager.Users.ToList();

    
        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);

            Users.Add(new UserViewModel
            {
                Email = user.Email ?? "(no email)",
                EmailConfirmed = user.EmailConfirmed,
             
                Roles = roles.ToList()
            });
        }
    }
}