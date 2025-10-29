
namespace BackendEcommerce.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Products listed within shops.
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Brand { get; set; }
        public string Status { get; set; } = "active";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Shop Shop { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<Media> Media { get; set; } = new List<Media>();
    }
}