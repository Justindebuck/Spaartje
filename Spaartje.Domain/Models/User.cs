namespace Spaartje.Domain.Models;


public class User
{
    // Stores the unique identifier for this user.
    //  Defaults to an empty string to avoid null values, but will be set to a real value when the user is created.
    public string Id { get; set; } = string.Empty;

    // The user's email address. Also used as their username.
    public string Email { get; set; } = string.Empty;

    // Whether the user's email has been confirmed.
    public bool EmailConfirmed { get; set; }

    // The roles this user belongs to (e.g. ["Admin"] or []).
    // A List because a user could theoretically have multiple roles.
    // Creates an empty list by default.
    public List<string> Roles { get; set; } = new();
}