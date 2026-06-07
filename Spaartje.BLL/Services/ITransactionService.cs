using Spaartje.Domain.Models;

namespace Spaartje.BLL.Services;

public interface ITransactionService
{
    Task<List<Transaction>> GetTransactionsForUserAsync(string userId);
    Task<Transaction?> GetByIdAsync(int id);
    Task CreateTransactionAsync(decimal amount, string description,
        DateTime date, TransactionType type, int categoryId, string userId);
    Task DeleteTransactionAsync(int transactionId, string userId);

    Task UpdateTransactionAsync(int transactionId, decimal amount, string description,
        DateTime date, TransactionType type, int categoryId, string userId);
}