namespace Spaartje.Domain.Models;

public class GroupTransaction
{
    public int Id { get; set; }

    // Which group this transaction belongs to
    public int GroupId { get; set; }

    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public TransactionType Type { get; set; }

    // Which user added this transaction
    public string UserId { get; set; } = string.Empty;
}