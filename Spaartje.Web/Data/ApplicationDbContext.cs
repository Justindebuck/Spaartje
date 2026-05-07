// WEB/Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WEB.Models;

namespace WEB.Data;

// WHY IdentityDbContext<ApplicationUser>?
// This base class automatically creates ALL Identity tables:
// AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, etc.
// Passing ApplicationUser as the generic type tells EF Core to use
// YOUR extended user model for the AspNetUsers table.
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Add your domain DbSets here later:
    // public DbSet<FinancialReport> FinancialReports => Set<FinancialReport>();
    // public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // ALWAYS call base first — Identity sets up its own table
        // configurations here. Skipping this breaks Identity entirely.
        base.OnModelCreating(builder);

        // Optional: rename default Identity tables to cleaner names
        builder.Entity<ApplicationUser>().ToTable("Users");

        // You can add indexes, constraints, etc. here later
    }
}