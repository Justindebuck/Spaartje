using System.Transactions;

namespace Spaartje.Domain.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public List<Transaction> Transactions { get; set; } = new();
    }
}