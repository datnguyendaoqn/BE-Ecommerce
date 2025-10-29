namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Merchant shops owned by users.
    /// </summary>
    public class Shop
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Logo { get; set; }
        public string Status { get; set; } = "active";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public User Owner { get; set; } = null!;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}