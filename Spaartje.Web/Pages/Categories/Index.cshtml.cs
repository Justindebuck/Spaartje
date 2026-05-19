using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Spaartje.BLL.Services;
using Spaartje.Domain.Models;

namespace Spaartje.Web.Pages.Categories;

// Only logged-in users can manage their categories.
[Authorize]
public class IndexModel : PageModel
{
    private readonly ICategoryService _categoryService;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(ICategoryService categoryService, UserManager<IdentityUser> userManager)
    {
        _categoryService = categoryService;
        _userManager = userManager;
    }

    // The list of categories shown in the table.
    public List<Category> Categories { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    // OnGetAsync loads the categories when the page is visited.
    public async Task OnGetAsync()
    {
        await LoadCategoriesAsync();
    }

    // OnPostAsync handles the "Add Category" form submission.
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }

        // Get the currently logged-in user's ID.
        // We need this to associate the new category with the correct user.
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return RedirectToPage("/Login");

        await _categoryService.CreateCategoryAsync(Input.Name, Input.Description, userId);

        SuccessMessage = $"Category '{Input.Name}' added successfully.";

        // Clear the input after successful submission.
        Input = new InputModel();
        ModelState.Clear();

        await LoadCategoriesAsync();
        return Page();
    }

    // OnPostDeleteAsync handles the delete button.
    // The method name suffix "Delete" matches asp-page-handler="Delete" in the form.
    public async Task<IActionResult> OnPostDeleteAsync(int categoryId)
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return RedirectToPage("/Login");

        await _categoryService.DeleteCategoryAsync(categoryId, userId);

        // Redirect to the same page after deletion.
        // This prevents the "resubmit form?" browser warning on refresh.
        return RedirectToPage();
    }

    // Private helper to avoid repeating the load logic.
    private async Task LoadCategoriesAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId != null)
        {
            Categories = await _categoryService.GetCategoriesForUserAsync(userId);
        }
    }
}