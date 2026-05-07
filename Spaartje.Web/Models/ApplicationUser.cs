// WEB/Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace WEB.Models;

// WHY extend IdentityUser?
// IdentityUser gives you Id, Email, PasswordHash, etc. for free.
// By extending it, you can add domain-specific fields (FirstName,
// LastName, etc.) without touching the Identity internals.
public class ApplicationUser : IdentityUser
{
    // Add your custom fields here
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Future fields you might want:
    // public string? ProfilePictureUrl { get; set; }
    // public string? Department { get; set; }
    // public bool IsActive { get; set; } = true;

    // Computed helper (not stored in DB)
    public string FullName => $"{FirstName} {LastName}".Trim();
}