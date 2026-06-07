using Spaartje.Domain.Models;

namespace Spaartje.DAL.Repositories;

public interface ITransactionRepository
{
    // Get all transactions for a user, newest first.
    Task<List<Transaction>> GetTransactionsByUserIdAsync(string userId);

    // Get all transactions for a user in a specific category.
    Task<List<Transaction>> GetTransactionsByUserIdAndCategoryAsync(string userId, int categoryId);

    // Get a single transaction by ID.
    Task<Transaction?> GetByIdAsync(int id);

    // Save a new transaction.
    Task AddAsync(Transaction transaction);

    // Delete a transaction.
    Task DeleteAsync(Transaction transaction);

   // Update an existing transaction
    Task UpdateAsync(Transaction transaction);
}