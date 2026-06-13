using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Spaartje.BLL.Services;
using Spaartje.Domain.Models;

namespace Spaartje.Web.Pages.Groups;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IGroupService _groupService;
   

    public DetailsModel(IGroupService groupService)
    {
        _groupService = groupService;
        
    }

    // The group Id comes from the URL: /Groups/Details?id=1
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Group? Group { get; set; }
    public List<GroupTransaction> Transactions { get; set; } = new();
    public string CurrentUserId { get; set; } = string.Empty;
    public bool IsManager { get; set; }

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    // For the add transaction form
    [BindProperty]
    public TransactionInputModel TransactionInput { get; set; } = new();

    // For the invite member form
    [BindProperty]
    public InviteInputModel InviteInput { get; set; } = new();

    public List<SelectListItem> TransactionTypeOptions { get; set; } = new()
    {
        new SelectListItem("Income", "0"),
        new SelectListItem("Expense", "1")
    };

    public class TransactionInputModel
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
    }

    public class InviteInputModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;
    }

    // Runs when the page loads
    public async Task<IActionResult> OnGetAsync()
    {
        return await LoadPageAsync();
    }

    // Runs when the Add Transaction form is submitted
    public async Task<IActionResult> OnPostAddTransactionAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return RedirectToPage("/Account/Login");
        var userId = int.Parse(userIdClaim);
        // Only validate the transaction fields
        if (!ModelState["TransactionInput.Description"]!.Errors.Any() == false ||
            !ModelState["TransactionInput.Amount"]!.Errors.Any() == false ||
            !ModelState["TransactionInput.Date"]!.Errors.Any() == false)
        {
            await LoadPageAsync();
            return Page();
        }

        if (TransactionInput.Amount == null)
        {
            ErrorMessage = "Amount is required.";
            await LoadPageAsync();
            return Page();
        }

        var error = await _groupService.AddTransactionAsync(
            Id,
            TransactionInput.Amount!.Value,
            TransactionInput.Description,
            TransactionInput.Date,
            TransactionInput.Type,
            userId);

        if (error != null)
        {
            ErrorMessage = error;
        }
        else
        {
            SuccessMessage = "Transaction added successfully.";
            TransactionInput = new TransactionInputModel();
            ModelState.Clear();
        }

        await LoadPageAsync();
        return Page();
    }

    // Runs when the Invite Member form is submitted
    public async Task<IActionResult> OnPostInviteMemberAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return RedirectToPage("/Account/Login");
        var userId = int.Parse(userIdClaim);

        var error = await _groupService.AddMemberByEmailAsync(
            Id,
            InviteInput.Email,
            userId);

        if (error != null)
        {
            ErrorMessage = error;
        }
        else
        {
            SuccessMessage = $"{InviteInput.Email} has been added to the group.";
            InviteInput = new InviteInputModel();
            ModelState.Clear();
        }

        await LoadPageAsync();
        return Page();
    }

    // Runs when the Remove Member button is clicked
    public async Task<IActionResult> OnPostRemoveMemberAsync(int memberUserId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return RedirectToPage("/Account/Login");
        var userId = int.Parse(userIdClaim);

        await _groupService.RemoveMemberAsync(Id, memberUserId, userId);

        return RedirectToPage(new { id = Id });
    }

    // Runs when the Delete Group button is clicked
    public async Task<IActionResult> OnPostDeleteGroupAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return RedirectToPage("/Account/Login");
        var userId = int.Parse(userIdClaim);
     ;

        await _groupService.DeleteGroupAsync(Id, userId);

        return RedirectToPage("/Groups/Index");
    }

    // Private helper — loads all the data the page needs
    private async Task<IActionResult> LoadPageAsync()
    {
         var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return RedirectToPage("/Account/Login");
        var userId = int.Parse(userIdClaim);

        CurrentUserId = userIdClaim;

        Group = await _groupService.GetGroupByIdAsync(Id, userId);

        // If group not found or user is not a member, send them back
        if (Group == null)
            return RedirectToPage("/Groups/Index");

        IsManager = Group.OwnerId == userId;

        Transactions = await _groupService.GetTransactionsAsync(Id, userId);

        return Page();
    }
}