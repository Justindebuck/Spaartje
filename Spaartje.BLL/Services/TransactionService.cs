using Spaartje.DAL.Repositories;
using Spaartje.Domain.Models;

namespace Spaartje.BLL.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<List<Transaction>> GetTransactionsForUserAsync(string userId)
    {
        return await _transactionRepository.GetTransactionsByUserIdAsync(userId);
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _transactionRepository.GetByIdAsync(id);
    }

    public async Task CreateTransactionAsync(decimal amount, string description,
        DateTime date, TransactionType type, int categoryId, string userId)
    {
        // Business rule: amount must be positive.
        // A negative expense doesn't make sense — the Type field already
        // indicates whether it is income or expense.
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        var transaction = new Transaction
        {
            Amount = amount,
            Description = description,
            Date = date,
            Type = type,
            CategoryId = categoryId,
            UserId = userId
        };

        await _transactionRepository.AddAsync(transaction);
    }

    public async Task DeleteTransactionAsync(int transactionId, string userId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);

        // Business rule: only the owner can delete their own transaction.
        if (transaction == null || transaction.UserId != userId)
            return;

        await _transactionRepository.DeleteAsync(transaction);
    }
}