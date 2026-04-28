using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Spaartje.DAL.Dblink;

public class AppUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}