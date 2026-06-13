namespace Spaartje.Domain.Models;

public class GroupMember
{
    public int Id { get; set; }

    // Which group this member belongs to
    public int GroupId { get; set; }

    // Which user this is
     public int UserId { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}