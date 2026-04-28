
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spaartje.Domain.Models;
public class SpaartjeDbContext : IdentityDbContext<User>
{
    public SpaartjeDbContext(DbContextOptions<SpaartjeDbContext> options) : base(options) { }

  
}