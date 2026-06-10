
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

    public DbSet<Transaction> Transactions { get; set; }

     public DbSet<Group> Groups { get; set; }                       
    public DbSet<GroupMember> GroupMembers { get; set; }           
    public DbSet<GroupTransaction> GroupTransactions { get; set; } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>{
            
            entity.Property(c => c.Name).IsRequired();

             entity.Property(c => c.Name).HasMaxLength(100);

             entity.Property(c => c.UserId).IsRequired();

             entity.Property(c => c.BudgetLimit)
          .HasColumnType("decimal(18,2)");

            


        
        });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
    

                entity.Property(t => t.Description).HasMaxLength(250);
                entity.Property(t => t.UserId).IsRequired();
    
                entity.HasOne(t => t.Category)
                    .WithMany(c => c.Transactions)
                    .HasForeignKey(t => t.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            
            modelBuilder.Entity<Group>()
                .Property(g => g.BudgetLimit)
                .HasColumnType("decimal(18,2)");

        
             modelBuilder.Entity<GroupTransaction>()
                .Property(t => t.Amount)
                 .HasColumnType("decimal(18,2)");

      
            modelBuilder.Entity<GroupMember>()
                .HasOne<Group>()
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

        
            modelBuilder.Entity<GroupTransaction>()
                .HasOne<Group>()
                .WithMany(g => g.Transactions)
                .HasForeignKey(gt => gt.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
    }
            
}