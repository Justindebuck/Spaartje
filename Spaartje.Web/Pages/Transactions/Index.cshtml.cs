using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Spaartje.BLL.Services;
using Spaartje.Domain.Models;

namespace Spaartje.Web.Pages.Transactions;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ITransactionService _transactionService;
    private readonly ICategoryService _categoryService;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(
        ITransactionService transactionService,
        ICategoryService categoryService,
        UserManager<IdentityUser> userManager)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;
        _userManager = userManager;
    }

    public List<Transaction> Transactions { get; set; } = new();
    public List<Category> Categories { get; set; } = new();

    // SelectListItem is an ASP.NET type used to populate <select> dropdowns.
    // Each item has a Text (shown to user) and a Value (sent with the form).
    public List<SelectListItem> CategoryOptions { get; set; } = new();

    // This creates dropdown options from the TransactionType enum automatically.
    public List<SelectListItem> TransactionTypeOptions { get; set; } = new()
    {
        new SelectListItem("Income", "0"),
        new SelectListItem("Expense", "1")
    };

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Description is required")]
        [MaxLength(250)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal? Amount { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; } = DateTime.Today;

        public TransactionType Type { get; set; } = TransactionType.Expense;

        [Required(ErrorMessage = "Please select a category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDataAsync();

        if (!ModelState.IsValid)
            return Page();

        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return RedirectToPage("/Login");

        try
        {
            await _transactionService.CreateTransactionAsync(
                Input.Amount!.Value,
                Input.Description,
                Input.Date,
                Input.Type,
                Input.CategoryId,
                userId);

            SuccessMessage = "Transaction added successfully.";
            Input = new InputModel();
            ModelState.Clear();
            await LoadDataAsync();
        }
        catch (ArgumentException ex)
        {
            // Catch the business rule exception from TransactionService.
            ErrorMessage = ex.Message;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int transactionId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return RedirectToPage("/Login");

        await _transactionService.DeleteTransactionAsync(transactionId, userId);
        return RedirectToPage();
    }

    private async Task LoadDataAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return;

        Transactions = await _transactionService.GetTransactionsForUserAsync(userId);
        Categories = await _categoryService.GetCategoriesForUserAsync(userId);

        // Build the dropdown options from the loaded categories.
        CategoryOptions = Categories.Select(c => new SelectListItem
        {
            Text = c.Name,
            Value = c.Id.ToString()
        }).ToList();
    }
}