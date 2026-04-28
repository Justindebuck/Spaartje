
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spaartje.Domain.Models;
namespace Spaartje.DAL.Dblink;
public class SpaartjeDbContext : IdentityDbContext<AppUser>
{
    public SpaartjeDbContext(DbContextOptions<SpaartjeDbContext> options) : base(options) { }
}