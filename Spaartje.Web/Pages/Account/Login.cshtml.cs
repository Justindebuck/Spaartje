// Pages/Account/Login.cshtml.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEB.Models;
using System.ComponentModel.DataAnnotations;

namespace WEB.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public LoginInputModel LoginInput { get; set; } = new();

    [BindProperty]
    public RegisterInputModel RegisterInput { get; set; } = new();

    // Controls which tab is active after a failed submit
    public string ActiveTab { get; set; } = "login";

    // ── Input Models ──────────────────────────────────────────────────────────

    public class LoginInputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class RegisterInputModel
    {
        [Required, MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    public void OnGet() { }

    // WHY separate OnPostLogin / OnPostRegister?
    // Razor Pages uses the handler name in the form's asp-page-handler attribute.
    // This lets both forms POST to the same page URL but route to different methods,
    // so validation errors on one form don't bleed into the other.
    public async Task<IActionResult> OnPostLoginAsync()
    {
        // Only validate LoginInput fields, ignore RegisterInput
        var loginFields = ModelState.Keys
            .Where(k => !k.StartsWith(nameof(LoginInput)));

        foreach (var field in loginFields)
            ModelState.Remove(field);

        if (!ModelState.IsValid)
        {
            ActiveTab = "login";
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            LoginInput.Email,
            LoginInput.Password,
            LoginInput.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
            return RedirectToPage("/Dashboard/Index");

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked. Try again in 15 minutes.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
        }

        ActiveTab = "login";
        return Page();
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        // Only validate RegisterInput fields, ignore LoginInput
        var registerFields = ModelState.Keys
            .Where(k => !k.StartsWith(nameof(RegisterInput)));

        foreach (var field in registerFields)
            ModelState.Remove(field);

        if (!ModelState.IsValid)
        {
            ActiveTab = "register";
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = RegisterInput.Email,
            Email = RegisterInput.Email,
            FirstName = RegisterInput.FirstName,
            LastName = RegisterInput.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, RegisterInput.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToPage("/Dashboard/Index");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        ActiveTab = "register";
        return Page();
    }
}