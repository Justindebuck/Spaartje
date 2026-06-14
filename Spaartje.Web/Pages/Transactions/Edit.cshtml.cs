using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Spaartje.BLL.Services;
using Spaartje.Domain.Models;
using System.Security.Claims;

namespace Spaartje.Web.Pages.Transactions;

[Authorize]
public class EditModel : PageModel
{
    private readonly ITransactionService _transactionService;
    private readonly ICategoryService _categoryService;
  
    public EditModel(
        ITransactionService transactionService,
        ICategoryService categoryService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;
       
    }

    // The transaction Id comes from the URL: /Transactions/Edit?id=1
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    // Dropdown options for the category select
    public List<SelectListItem> CategoryOptions { get; set; } = new();

    // Dropdown options for the transaction type select
    public List<SelectListItem> TransactionTypeOptions { get; set; } = new()
    {
        new SelectListItem("Income", "0"),
        new SelectListItem("Expense", "1")
    };

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Description is required")]
        [MaxLength(250)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; } = DateTime.Today;

        public TransactionType Type { get; set; } = TransactionType.Expense;

        [Required(ErrorMessage = "Please select a category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }
    }

    // OnGetAsync runs when the user visits /Transactions/Edit?id=1
    // It loads the existing transaction and pre-fills the form
    public async Task<IActionResult> OnGetAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return RedirectToPage("/Login");
        var userId = int.Parse(userIdClaim);

        var transaction = await _transactionService.GetByIdAsync(Id);

        // If transaction does not exist or belongs to someone else, send them back
        if (transaction == null || transaction.UserId != userId)
            return RedirectToPage("/Transactions/Index");

        // Pre-fill the form with the existing values
        Input.Description = transaction.Description;
        Input.Amount      = transaction.Amount;
        Input.Date        = transaction.Date;
        Input.Type        = transaction.Type;
        Input.CategoryId  = transaction.CategoryId;

        await LoadCategoryOptionsAsync(userId);

        return Page();
    }

    // OnPostAsync runs when the user submits the edit form
    public async Task<IActionResult> OnPostAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return RedirectToPage("/Login");
        var userId = int.Parse(userIdClaim);

        await LoadCategoryOptionsAsync(userId);

        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _transactionService.UpdateTransactionAsync(
                Id,
                Input.Amount,
                Input.Description,
                Input.Date,
                Input.Type,
                Input.CategoryId,
                userId);

            return RedirectToPage("/Transactions/Index");
        }
        catch (ArgumentException ex)
        {
            // Catches the business rule exception from TransactionService
            ErrorMessage = ex.Message;
            return Page();
        }
    }

    // Private helper — loads the category dropdown
    private async Task LoadCategoryOptionsAsync(int userId)
    {
        var categories = await _categoryService.GetCategoriesForUserAsync(userId);

        CategoryOptions = categories.Select(c => new SelectListItem
        {
            Text  = c.Name,
            Value = c.Id.ToString()
        }).ToList();
    }
}