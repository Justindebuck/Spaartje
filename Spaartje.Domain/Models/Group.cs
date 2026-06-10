namespace Spaartje.Domain.Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? BudgetLimit { get; set; }

    // The user who created the group — they are the Manager
    public string OwnerId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // These lists let EF Core load the related members and transactions
    public List<GroupMember> Members { get; set; } = new();
    public List<GroupTransaction> Transactions { get; set; } = new();
}