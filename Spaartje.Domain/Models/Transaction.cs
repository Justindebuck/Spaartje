namespace Spaartje.Domain.Models
{
    public class Transaction
    {
        // The Id property is the primary key of the Transaction table in the database.
        public int Id { get; set; }
        
        // The Amount property represents the amount of money for this transaction.
        public decimal Amount { get; set; }

        // The Description property is an optional text field where the user can add details about the transaction.
        public string Description { get; set; } = string.Empty;

        // The Date property represents the date and time when the transaction occurred.

        public DateTime Date { get; set; }

        // The Type property indicates whether this transaction is an income or an expense.

        public TransactionType Type { get; set; }

        // The CategoryId property is a foreign key that links this transaction to a category.

        public int CategoryId { get; set; }

        // The Category property is a navigation property that allows us to access the related Category object directly from a Transaction object.

        public Category? Category { get; set; }

        // The UserId property is a foreign key that links this transaction to the user who created it.

        public int UserId { get; set; }
    }
}