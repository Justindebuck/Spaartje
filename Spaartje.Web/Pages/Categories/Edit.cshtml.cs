using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Spaartje.BLL.Services;

namespace Spaartje.Web.Pages.Categories;

[Authorize]
public class EditModel : PageModel
{
    private readonly ICategoryService _categoryService;

    public EditModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
       
    }

    // The category Id comes from the URL: /Categories/Edit?id=1
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Budget limit must be greater than zero")]
        public decimal? BudgetLimit { get; set; }
    }

    // OnGetAsync runs when the user visits /Categories/Edit?id=1
    // It loads the existing category and pre-fills the form
    public async Task<IActionResult> OnGetAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return RedirectToPage("/Account/Login");
        var userId = int.Parse(userIdClaim);

        var category = await _categoryService.GetByIdAsync(Id);

        // If category does not exist or belongs to someone else, send them back
        if (category == null || category.UserId != userId)
            return RedirectToPage("/Categories/Index");

        // Pre-fill the form with the existing values
        Input.Name = category.Name;
        Input.Description = category.Description;
        Input.BudgetLimit = category.BudgetLimit; 

        return Page();
    }

    // OnPostAsync runs when the user submits the edit form
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

       var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return RedirectToPage("/Account/Login");
        var userId = int.Parse(userIdClaim);

        await _categoryService.UpdateCategoryAsync(Id, Input.Name, Input.Description, userId);

        await _categoryService.SetBudgetLimitAsync(Id,Input.BudgetLimit,userId);

        // Go back to the categories list after saving
        return RedirectToPage("/Categories/Index");
    }
}