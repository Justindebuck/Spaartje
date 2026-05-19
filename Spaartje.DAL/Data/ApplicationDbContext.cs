
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spaartje.Domain.Models;

namespace Spaartje.DAL.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>{
            
            entity.Property(c => c.Name).IsRequired();

             entity.Property(c => c.Name).HasMaxLength(100);

             entity.Property(c => c.UserId).IsRequired();

            


        
        });
    }
            
}