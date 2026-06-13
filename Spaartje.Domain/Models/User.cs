namespace Spaartje.Domain.Models;


public class User
{
    // Stores the unique identifier for this user.
    //  Defaults to an empty string to avoid null values, but will be set to a real value when the user is created.
    public int Id { get; set; } 

    // The user's email address. Also used as their username.
    public string Email { get; set; } = string.Empty;

    public string Password{ get; set; } = string.Empty;
    public string Username{ get; set; } = string.Empty;
    public string role { get; set; } = "user"; // Default role is "user", can be changed to "admin" for admin users.

    

    
}