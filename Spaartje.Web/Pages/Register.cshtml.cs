
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spaartje.BLL.Services;

namespace Spaartje.Web.Pages;

public class RegisterModel : PageModel
{
    private readonly IUserService _userService;

    public RegisterModel(IUserService userService)
    {
        _userService = userService;
    }

    [BindProperty] public string Email    { get; set; } = string.Empty;
    [BindProperty] public string UserName { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
      var (success, error) = await _userService.RegisterAsync(Email, UserName, Password);

        if (!success)
        {
            ErrorMessage = error;
            return Page();
        }

        return RedirectToPage("/Dashboard");
    }
}