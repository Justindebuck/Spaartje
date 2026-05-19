namespace Spaartje.Domain.Models;

// An enum defines a fixed set of named constants.
// Stored in the database as integers: Income = 0, Expense = 1.
// In code we always use the name (TransactionType.Income),
// never the number — making the code readable.
public enum TransactionType
{
    Income,
    Expense
}