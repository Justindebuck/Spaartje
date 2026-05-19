using Microsoft.EntityFrameworkCore;
using Spaartje.DAL.Data;
using Spaartje.Domain.Models;

namespace Spaartje.DAL.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Transaction>> GetTransactionsByUserIdAsync(string userId)
    {
        return await _context.Transactions
            // Include() tells EF Core to also load the related Category object.
            // Without Include(), Transaction.Category would be null.
            // This produces a SQL JOIN between Transactions and Categories.
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            // Order newest transactions first.
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByUserIdAndCategoryAsync(
        string userId, int categoryId)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.CategoryId == categoryId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Transaction transaction)
    {
        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
    }
}